using System;

public sealed class ObjectChannels : Component, Component.ExecuteInEditor
{
	[Property, MakeDirty, WideMode]
	public Dictionary<string, Vector4> Channels { get; set; }

	private ModelRenderer mdl;

	protected override void OnEnabled()
	{
		base.OnEnabled();
	}

	protected override void OnStart()
	{
		if ( Channels.Any() )
		{
			mdl = Components.Get<ModelRenderer>();
			mdl.SceneObject.Batchable = false;
			BuildChannels();
		}

		base.OnStart();
	}


	[Button( "Update Channels" ), Order( -1 )]
	public void BuildChannels()
	{
		Dictionary<string, Vector4> updatedChannels = new Dictionary<string, Vector4>( Channels );
		foreach ( var channel in Channels )
		{
			var vec = channel.Value;
			switch ( channel.Key )
			{
				case "interpolated_world_position":
					vec = new Vector4( GameObject.WorldPosition );
					break;

				case "unique_id":
					vec = new Vector4( GetID() );
					break;
			}
			updatedChannels[channel.Key] = vec;

			mdl?.SceneObject.Attributes.Set( $"ObjectChannel_{channel.Key}", vec );
		}

		Channels = updatedChannels;
	}

	private float GetID()
	{
		return Random.Shared.Float();
		//return this.GameObject.Parent?.Children.IndexOf( this.GameObject ) ?? 0;
	}
}
