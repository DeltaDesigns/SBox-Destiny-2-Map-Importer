using System.Linq;
using Sandbox;
using Sandbox.Citizen;

[Icon("directions_run")]
public sealed class NoClip : Component
{
	[Sync] Angles EyeAngles { get; set; }
	[Sync] public Vector3 WishVelocity { get; set; }
	[Property, Sync] public float MoveSpeed { get; set; } = 1000;
	[Property, Sync] public bool FirstPerson { get; set; } = true;
	[Property, Sync] public int DistanceFromCamera { get; set; } = 200;
	[Property] public CitizenAnimationHelper AnimationHelper { get; set; }
	[Property] public float RunSpeed { get; set; } = 2000;

	public float GetSpeed()
	{
		if (Input.Down("run"))
		{
			return RunSpeed;
		}
		else
		{
			return MoveSpeed;
		}
	}
	protected override void OnStart()
	{
		if (!IsProxy)
		{
			var firstPerson = FileSystem.Data.ReadAllText("firstperson.txt").ToBool();
			FirstPerson = firstPerson;
			EyeAngles = Transform.Rotation.Angles();
		}
	}

	public void GetFirstPerson()
	{
		
	}

	protected override void OnUpdate()
	{
		BodyVis();
		if (!IsProxy)
		{
			BuildEyeAngles();
			Camera();
			if (Input.Pressed("3rd/1st Person Toggle"))
			{
				FirstPerson = !FirstPerson;
				FileSystem.Data.WriteAllText("firstperson.txt", FirstPerson.ToString());
			}
		}
	}

	protected override void OnFixedUpdate()
	{
		Anims();
		AnimationHelper.Target.Transform.Rotation = Rotation.Slerp(AnimationHelper.Target.Transform.Rotation, new Angles(0, EyeAngles.yaw, 0).ToRotation(), Time.Delta * 10);
		if (!IsProxy)
		{
			Move();
		}
	}

	public void BuildEyeAngles()
	{
		var ee = EyeAngles;
		ee += Input.AnalogLook;
		ee.pitch = ee.pitch.Clamp( -89, 89 );
		ee.roll = 0;
		EyeAngles = ee;
	}

	public void Move()
	{
		WishVelocity = new Angles(EyeAngles.pitch, EyeAngles.yaw, 0).ToRotation() * Input.AnalogMove.Normal;
		WishVelocity *= GetSpeed();
		if (!WishVelocity.IsNearlyZero())
		{
			Transform.Position += WishVelocity * Time.Delta;
		}
	}

	public void Camera()
	{
		var camera = Scene.GetAllComponents<CameraComponent>().FirstOrDefault( x => x.IsMainCamera );
		camera.FieldOfView = Preferences.FieldOfView;
		var lookDirection = EyeAngles.ToRotation();
		var center = Transform.Position + Vector3.Up * 64;
		//Trace to see if the camera is inside a wall
		if (!FirstPerson)
		{
			camera.Transform.Position = center + lookDirection.Backward * DistanceFromCamera;
		}
		else
		{
			var targetPos = Transform.Position + Vector3.Up * 64;
			camera.Transform.Position = targetPos;
		}
	
		camera.Transform.Rotation = lookDirection;
		
	}

	public void BodyVis()
	{
		var target = AnimationHelper.Target;
		if (FirstPerson)
		{
			var bodyVis = IsProxy ? ModelRenderer.ShadowRenderType.On : ModelRenderer.ShadowRenderType.ShadowsOnly;
			target.RenderType = bodyVis;
			foreach (var child in target.Components.GetAll<SkinnedModelRenderer>(FindMode.InDescendants))
			{
				child.RenderType = bodyVis;
			}
		}
		else
		{
			target.RenderType = ModelRenderer.ShadowRenderType.On;
			foreach (var child in target.Components.GetAll<SkinnedModelRenderer>(FindMode.InDescendants))
			{
				child.RenderType = ModelRenderer.ShadowRenderType.On;
			}
		}
	}

	public void Anims()
	{
		AnimationHelper.WithVelocity(WishVelocity);
		AnimationHelper.WithWishVelocity(WishVelocity);
		AnimationHelper.IsNoclipping = true;
	}
}
