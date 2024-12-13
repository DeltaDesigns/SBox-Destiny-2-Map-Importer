public sealed class Bytecode : Component, Component.ExecuteInEditor
{
	[Property, Hide] public byte[] BytecodeArray { get; set; }
	[Property, Hide] public Vector4[] Constants { get; set; }
	[Property] public List<Light> Lights { get; set; }
	[Property] public bool Debug { get; set; }

	private TfxBytecodeInterpreter InterpretedBytecode { get; set; }

	protected override void OnStart()
	{
		GlobalChannelDefaults.GetGlobalChannelDefaults();
		Lights = Components.GetAll<Light>( FindMode.EverythingInChildren ).ToList();
		InterpretedBytecode = new( TfxBytecodeOp.ParseAll( BytecodeArray ) );
		InterpretedBytecode.Light = GameObject.Name;
	}

	protected override async void OnUpdate()
	{
		if ( Constants == null || !Constants.Any() )
			return;

		var a = await InterpretedBytecode.Evaluate( Constants );
		if ( a.Count == 0 )
			return;

		var col = a.Values.Last();
		if ( Debug ) Log.Info( col );

		var color = new Color( col.X, col.Y, col.Z, 1 );
		foreach ( var light in Lights )
		{
			light.LightColor = color.Darken( 0.75f );
			//if ( light is PointLight lightPointLight )
			//	lightPointLight.Attenuation = 10f;
		}
	}

	[Button( "Print Bytecode" )]
	public void PrintBytecode()
	{
		Log.Info( $"--Bytecode for Light {GameObject.Name}--" );
		foreach ( var op in InterpretedBytecode.Opcodes )
		{
			Log.Info( $"0x{op.op.AsInt():X} {op.op} : {TfxBytecodeOp.TfxToString( op, Constants )}" );
		}
	}
}
