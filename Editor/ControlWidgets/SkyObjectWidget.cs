//using Sandbox.UI;
//using System.IO;

// Eh fuck it I can't be bothered to do this

//[CustomEditor( typeof( SkyObject ) )]
//public class SkyObjectControlWidget : ControlObjectWidget
//{
//	// Whether or not this control supports multi-editing (if you have multiple GameObjects selected)
//	public override bool SupportsMultiEdit => false;
//	private Widget content;

//	public SkyObjectControlWidget( SerializedProperty property ) : base( property, true )
//	{
//		Layout = Layout.Column();
//		Layout.Margin = new Margin( 8, 5, 0, 4 );

//		Rebuild();
//	}

//	void Rebuild()
//	{
//		Layout.Clear( true );

//		content = new Widget();

//		var cs = new ControlSheet();
//		cs.IncludePropertyNames = true;
//		cs.Margin = new Margin( 0, 0, 0, 0 );

//		var modelName = Path.GetFileNameWithoutExtension( SerializedObject.GetProperty( "Model" ).GetValue<Model>()?.Name.ToUpper() );
//		cs.AddGroup( $"{modelName}", [SerializedObject.GetProperty( "Model" ), SerializedObject.GetProperty( "Transform" ), SerializedObject.GetProperty( "Order" )] );

//		content.Layout = Layout.Column();
//		content.Layout.Add( cs );
//		Layout.Add( content );
//	}

//	protected override void OnMultipleDifferentValues( bool state )
//	{
//		base.OnMultipleDifferentValues( state );
//		Log.Info( "test" );
//	}


//	protected override void OnPaint()
//	{
//		// Overriding and doing nothing here will prevent the default background from being painted
//	}
//}

