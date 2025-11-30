using Sandbox.Rendering;

public sealed class GlobalChannel : Component, Component.ExecuteInEditor
{
	[Property, MakeDirty]
	public string ChannelName { get; set; }
	[Property, MakeDirty]
	public int ChannelIndex { get; set; }

	[Property, MakeDirty, Change]
	public Vector4 Value { get; set; }

	[Property, MakeDirty]
	public List<byte> Bytecode { get; set; } = new();
	[Property, MakeDirty]
	public List<Vector4> Constants { get; set; } = new();

	[Property] public bool IsStatic { get; set; } = false;
	[Property, Group( "Debug" )] public bool DebugBytecode { get; set; } = false;

	public TfxBytecodeInterpreter InterpretedBytecode { get; set; }

	[Property]
	public GlobalChannelsController Controller { get; set; }

	public CommandList Commands;

	protected override void OnStart()
	{
		if ( Commands is null )
			Commands = new CommandList( $"Global Channel {ChannelIndex}" );

		Game.ActiveScene.Camera?.AddCommandList( Commands, Stage.AfterDepthPrepass );

		if ( Controller is null )
			Controller = GlobalChannelsController.Get();

		if ( !Bytecode.Any() && Constants.Any() )
			Value = Constants[0];

		Controller?.SetGlobalChannel( ChannelIndex, Value );

		InterpretedBytecode = new( TfxBytecodeOp.ParseAll( Bytecode.ToArray(), TfxBytecodeOp.BytecodeType.Sequencer ) );
		InterpretedBytecode.Name = GameObject.Name;
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();

		if ( Commands is null )
			Commands = new CommandList( $"Global Channel {ChannelIndex}" );

		if ( Controller is null )
			Controller = GlobalChannelsController.Get();
	}

	protected override void OnDirty()
	{
		base.OnDirty();

		Commands?.Reset();
		Commands?.GlobalAttributes.Set( $"GlobalChannel{ChannelIndex}", Value );
	}

	private void OnValueChanged( Vector4 oldValue, Vector4 newValue )
	{
		Commands?.Reset();
		Commands?.GlobalAttributes.Set( $"GlobalChannel{ChannelIndex}", newValue );
	}

	protected override async void OnUpdate()
	{
		if ( IsStatic || Constants == null || !Constants.Any() )
			return;

		var a = await InterpretedBytecode.Evaluate( Constants.ToArray() );
		if ( a.Count == 0 )
			return;

		// Run Evaluate once before setting IsStatic
		IsStatic = Bytecode.Count <= 4;

		//Value = a.Values.Last();
		Controller?.SetGlobalChannel( ChannelIndex, a.Values.Last() );

		if ( DebugBytecode )
			Log.Info( $"{ChannelName}: {Value}" );
	}

	[Button( "Print Bytecode" ), Group( "Debug" )]
	private void PrintBytecode()
	{
		if ( InterpretedBytecode is null )
			return;

		//Log.Info( $"{ChannelName}: {string.Join( ", ", InterpretedBytecode.Opcodes.Select( x => x.op ) )}" );
		Log.Info( $"{ChannelName} ({ChannelIndex}): " );
		foreach ( var op in InterpretedBytecode.Opcodes )
		{
			Log.Info( $"{op.op}: {TfxBytecodeOp.TfxToString( op, Constants.ToArray() )}" );
		}
	}

	protected override void OnDisabled()
	{
		if ( Commands is not null )
		{
			Commands.Reset();
			Game.ActiveScene.Camera?.RemoveCommandList( Commands );
			Commands = null;
		}
	}
	protected override void OnDestroy()
	{
		if ( Commands is not null )
		{
			Commands.Reset();
			Game.ActiveScene.Camera?.RemoveCommandList( Commands );
			Commands = null;
		}
	}
}
