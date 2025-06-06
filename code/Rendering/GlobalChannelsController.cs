using Sandbox.Rendering;

public sealed class GlobalChannelsController : Component, Component.ExecuteInEditor
{
	//[Property, MakeDirty, WideMode, Order( 1 )]
	//public Dictionary<int, Vector4> Channels { get; set; } = new();

	[Property]
	public Dictionary<int, GlobalChannel> ChannelComps { get; set; }

	private Dictionary<int, Vector4> _channelValues { get; set; } = new();

	public CommandList Commands;

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
		if ( Commands is null )
			Commands = new CommandList( "Global Channels" );

		Fill();
	}

	protected override void OnStart()
	{
		base.OnStart();

		if ( Commands is null )
			Commands = new CommandList( "Global Channels" );

		Fill();
		Game.ActiveScene.Camera?.AddCommandList( Commands, Stage.AfterDepthPrepass );
	}

	public void Fill()
	{
		if ( ChannelComps is null || !ChannelComps.Any() )
			ChannelComps = this.GameObject.Children
				.Select( x => x.GetComponent<GlobalChannel>() )
				.ToDictionary( x => x.ChannelIndex, x => x );
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

	public Vector4? Get( string name )
	{
		if ( ChannelComps.Any( x => x.Value.ChannelName == name ) )
		{
			var channel = ChannelComps.First( x => x.Value.ChannelName == name ).Value;
			return channel?.Value ?? Vector4.Zero;
		}

		Log.Warning( $"Get: Global Channel '{name}' not found." );
		return null;
	}

	public void Set( string name, Vector4 value )
	{
		if ( ChannelComps.Any( x => x.Value.ChannelName == name ) )
		{
			var channel = ChannelComps.First( x => x.Value.ChannelName == name ).Value;
			channel.Value = value;
			return;
		}

		Log.Warning( $"Set: Global Channel '{name}' not found." );
	}

	public void SetGlobalChannel( int index, Vector4 value )
	{
		if ( !ChannelComps.ContainsKey( index ) )
		{
			Log.Info( $"SetGlobalChannel: Global Channel {index} not found" );
			return;
		}

		if ( !ChannelComps[index].Value.Equals( value ) || !_channelValues.ContainsKey( index ) )
		{
			_channelValues.TryAdd( index, value );
			ChannelComps[index].Value = value;

			Commands?.GlobalAttributes.Set( $"GlobalChannel{index}", value );
			Log.Info( $"SetGlobalChannel: GlobalChannel{index} set to {value}" );
		}
		//Commands?.GlobalAttributes.Set( $"GlobalChannel{index}", value );

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



