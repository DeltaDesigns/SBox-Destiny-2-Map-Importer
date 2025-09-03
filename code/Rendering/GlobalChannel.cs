public sealed class GlobalChannel : Component, Component.ExecuteInEditor
{
	[Property, MakeDirty]
	public string ChannelName { get; set; }
	[Property, MakeDirty]
	public int ChannelIndex { get; set; }
	[Property, MakeDirty]
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

	protected override void OnStart()
	{
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
		if ( Controller is null )
			Controller = GlobalChannelsController.Get();
	}

	protected override void OnDirty()
	{
		base.OnDirty();
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

		Log.Info( $"{ChannelName}: {string.Join( ", ", InterpretedBytecode.Opcodes.Select( x => x.op ) )}" );
	}
}
