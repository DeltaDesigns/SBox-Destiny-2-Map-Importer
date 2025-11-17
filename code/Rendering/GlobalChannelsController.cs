public sealed class GlobalChannelsController : Component, Component.ExecuteInEditor
{
	//[Property, MakeDirty, WideMode, Order( 1 )]
	//public Dictionary<int, Vector4> Channels { get; set; } = new();
	[Property]
	public Texture LUT { get; set; } = Texture.Load( "Pipelines/Textures/lut_temp.vtex" );

	[Property]
	public Dictionary<int, GlobalChannel> ChannelComps { get; set; }

	public List<Vector4> MiscValues { get; set; } = new();

	private Dictionary<int, Vector4> _channelValues { get; set; } = new();
	private Dictionary<string, int> _channelNameToIndex { get; set; } = new();

	//[Property]
	//public bool UpdateInRealTime { get; set; } = false;

	public static GlobalChannelsController Get()
	{
		Game.ActiveScene.Components.TryGet<GlobalChannelsController>( out GlobalChannelsController _globalChannels, FindMode.InDescendants );
		return _globalChannels;
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();
		Fill();
	}

	protected override void OnStart()
	{
		base.OnStart();
		Fill();
	}

	public void Fill()
	{
		// Fill with 256 Vector4.Zeros if empty
		if ( MiscValues.Count == 0 )
			MiscValues = Enumerable.Repeat( Vector4.Zero, 256 ).ToList();

		if ( ChannelComps is null || !ChannelComps.Any() )
		{
			ChannelComps = this.GameObject.Children
				.Select( x => x.GetComponent<GlobalChannel>() )
				.ToDictionary( x => x.ChannelIndex, x => x );
		}
		_channelNameToIndex = ChannelComps.ToDictionary( x => x.Value.ChannelName, x => x.Key );
	}

	protected override void OnDirty()
	{
		base.OnDirty();
	}

	public Vector4 Get( int index )
	{
		//if ( Channels.TryGetValue( index, out Vector4 value ) )
		if ( ChannelComps.TryGetValue( index, out GlobalChannel channel ) && channel != null )
			return channel.Value;

		return Vector4.Zero;
	}

	public GlobalChannel GetChannel( string name )
	{
		if ( _channelNameToIndex.TryGetValue( name, out int index ) && ChannelComps.TryGetValue( index, out GlobalChannel channel ) )
		{
			return channel;
		}

		Log.Warning( $"GetChannel: Global Channel '{name}' not found." );
		return null;
	}

	public Vector4? Get( string name )
	{
		if ( _channelNameToIndex.TryGetValue( name, out int index ) && ChannelComps.TryGetValue( index, out GlobalChannel channel ) )
		{
			return channel?.Value ?? Vector4.Zero;
		}

		Log.Warning( $"Get: Global Channel '{name}' not found." );
		return null;
	}

	public void Set( string name, Vector4 value )
	{
		if ( _channelNameToIndex.TryGetValue( name, out int index ) && ChannelComps.TryGetValue( index, out GlobalChannel channel ) )
		{
			channel.Value = value;
			return;
		}

		Log.Warning( $"Set: Global Channel '{name}' not found." );
	}

	public void SetGlobalChannel( int index, Vector4 value )
	{
		if ( !ChannelComps.TryGetValue( index, out GlobalChannel channel ) )
		{
			//Log.Info( $"SetGlobalChannel: Global Channel {index} not found." );
			return;
		}

		if ( !_channelValues.TryGetValue( index, out Vector4 currentValue ) || !currentValue.Equals( value ) )
		{
			_channelValues[index] = value;
			channel.Value = value;

			//Log.Info( $"SetGlobalChannel: Global Channel {index} updated to {value}." );
		}
	}
}



