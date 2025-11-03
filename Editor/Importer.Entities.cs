using System.IO;
using System.Numerics;
using System.Text.Json;

public partial class DestinyImporter : EditorTool
{
	private static void ImportEntities( List<string> mapList )
	{
		if ( mapList.Count == 0 )
			return;

		var entMapRoot = scene.CreateObject();
		entMapRoot.Name = "Entities";

		foreach ( string path in mapList )
		{
			JsonDocument cfg = JsonDocument.Parse( File.ReadAllText( path ) );
			if ( cfg.RootElement.GetProperty( "Instances" ).EnumerateObject().Count() == 0 )
				continue;

			string fileName = Path.GetFileNameWithoutExtension( path );
			fileName = fileName.Substring( 0, fileName.Length - 5 ); //removes "_info" from the name

			var group = scene.CreateObject();
			group.Name = fileName;
			group.Parent = entMapRoot;
			group.NetworkMode = NetworkMode.Never;

			foreach ( JsonProperty model in cfg.RootElement.GetProperty( "Instances" ).EnumerateObject() )
			{
				string modelName = GetModelPath( ImportType.Entity, model.Name );
				var mdl = Model.Load( modelName );
				if ( IsInvalidModel( mdl ) )
					continue;

				int i = 0;
				var entMapParent = scene.CreateObject();
				entMapParent.Name = $"{model.Name}";
				entMapParent.Parent = group;
				entMapParent.NetworkMode = NetworkMode.Never;

				var channels = GetObjectChannels( cfg, model.Name, path );

				foreach ( JsonElement instance in model.Value.EnumerateArray() )
				{
					Vector3 position = new Vector3(
						instance.GetProperty( "Translation" )[0].GetSingle() * 39.37f,
						instance.GetProperty( "Translation" )[1].GetSingle() * 39.37f,
						instance.GetProperty( "Translation" )[2].GetSingle() * 39.37f );

					Quaternion quatRot = new Quaternion
					{
						X = instance.GetProperty( "Rotation" )[0].GetSingle(),
						Y = instance.GetProperty( "Rotation" )[1].GetSingle(),
						Z = instance.GetProperty( "Rotation" )[2].GetSingle(),
						W = instance.GetProperty( "Rotation" )[3].GetSingle()
					};

					Vector3 scale = new Vector3(
						instance.GetProperty( "Scale" )[0].GetSingle(),
						instance.GetProperty( "Scale" )[1].GetSingle(),
						instance.GetProperty( "Scale" )[2].GetSingle() );

					var entMapPart = scene.CreateObject();
					entMapPart.Name = $"{model.Name}_{i}";
					entMapPart.Parent = entMapParent;

					entMapPart.WorldPosition = position;
					entMapPart.WorldRotation = quatRot;
					entMapPart.WorldScale = scale;

					entMapPart.NetworkMode = NetworkMode.Never;

					var mdlRender = entMapPart.Components.GetOrCreate<ModelRenderer>();
					mdlRender.Model = mdl;

					if ( channels.Any() )
					{
						var objChans = entMapPart.Components.GetOrCreate<ObjectChannels>();
						objChans.Channels = channels;
					}

					if ( _assignObjectCol )
					{
						var col = entMapPart.Components.GetOrCreate<ModelCollider>();
						col.Static = true;
						col.Model = mdlRender.Model;
					}

					i++;
				}
			}
		}
	}

	private static Dictionary<string, Vector4> GetObjectChannels( JsonDocument cfg, string modelHash, string path )
	{
		var matPath = $"{Path.GetDirectoryName( path )}/Materials/";

		Dictionary<string, Vector4> channelNames = new();
		foreach ( var mat in cfg.RootElement.GetProperty( "Parts" ).GetProperty( modelHash ).GetProperty( "PartMaterials" ).EnumerateObject() )
		{
			var matFilePath = $"{matPath}{mat.Value}.json";
			if ( !File.Exists( matFilePath ) )
				continue;

			JsonDocument matJson = JsonDocument.Parse( File.ReadAllText( matFilePath ) );
			foreach ( JsonProperty channel in matJson.RootElement.GetProperty( "UsedChannelNames" ).EnumerateObject() )
			{
				channelNames.TryAdd( channel.Value.ToString(), new( 1, 1, 1, 1 ) );
			}
		}

		return channelNames;
	}
}

