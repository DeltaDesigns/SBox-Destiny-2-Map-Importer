using Sandbox;
using System;
using System.Linq;

[Icon( "directions_run" )]
public sealed class NoClip : Component
{
	public Angles EyeAngles { get; set; }
	public Vector3 WishVelocity { get; set; }
	[Property] public float MoveSpeed { get; set; } = 1000;
	[Property] public float RunSpeed { get; set; } = 2000;
	[Property, Range( 0, 1 )] public float CrouchSpeed { get; set; } = 0.25f;
	[Property] public bool FirstPerson { get; set; } = true;
	[Property] public int DistanceFromCamera { get; set; } = 200;
	public float MouseMulti = 1;
	//[Property] public CitizenAnimationHelper AnimationHelper { get; set; }

	public float GetSpeed()
	{
		if ( Input.MouseWheel.y != 0.0f )
		{
			MouseMulti = Math.Clamp( MouseMulti + (Input.MouseWheel.y * 0.1f), 0, 10 );
			Log.Info( MouseMulti );
		}
		if ( Input.Down( "run" ) )
		{
			return RunSpeed * MouseMulti;
		}
		else if ( Input.Down( "duck" ) )
		{
			return (MoveSpeed * CrouchSpeed) * MouseMulti;
		}
		else
		{
			return MoveSpeed * MouseMulti;
		}
	}
	protected override void OnStart()
	{
		if ( !IsProxy )
		{
			EyeAngles = WorldRotation.Angles();
		}
	}

	public void GetFirstPerson()
	{

	}

	protected override void OnUpdate()
	{
		BodyVis();
		if ( !IsProxy )
		{
			BuildEyeAngles();
			Camera();
		}
	}

	protected override void OnFixedUpdate()
	{
		//Anims();
		//AnimationHelper.Target.Transform.Rotation = Rotation.Slerp(AnimationHelper.Target.Transform.Rotation, new Angles(0, EyeAngles.yaw, 0).ToRotation(), Time.Delta * 10);
		if ( !IsProxy )
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
		WishVelocity = new Angles( EyeAngles.pitch, EyeAngles.yaw, 0 ).ToRotation() * Input.AnalogMove.Normal;
		WishVelocity *= Math.Clamp( GetSpeed(), 0f, 100000 );
		if ( !WishVelocity.IsNearlyZero() )
		{
			WorldPosition += WishVelocity * Time.Delta;
		}
	}

	public void Camera()
	{
		var camera = Scene.GetAllComponents<CameraComponent>().FirstOrDefault( x => x.IsMainCamera );
		//camera.FieldOfView = Preferences.FieldOfView;
		var lookDirection = EyeAngles.ToRotation();
		var center = WorldPosition + Vector3.Up * 64;
		//Trace to see if the camera is inside a wall
		if ( !FirstPerson )
		{
			camera.WorldPosition = center + lookDirection.Backward * DistanceFromCamera;
		}
		else
		{
			var targetPos = WorldPosition + Vector3.Up * 64;
			camera.WorldPosition = targetPos;
		}

		camera.WorldRotation = lookDirection;

	}

	public void BodyVis()
	{
		//var target = AnimationHelper.Target;
		//if (FirstPerson)
		//{
		//	var bodyVis = IsProxy ? ModelRenderer.ShadowRenderType.On : ModelRenderer.ShadowRenderType.ShadowsOnly;
		//	target.RenderType = bodyVis;
		//	foreach (var child in target.Components.GetAll<SkinnedModelRenderer>(FindMode.InDescendants))
		//	{
		//		child.RenderType = bodyVis;
		//	}
		//}
		//else
		//{
		//	target.RenderType = ModelRenderer.ShadowRenderType.On;
		//	foreach (var child in target.Components.GetAll<SkinnedModelRenderer>(FindMode.InDescendants))
		//	{
		//		child.RenderType = ModelRenderer.ShadowRenderType.On;
		//	}
		//}
	}

	public void Anims()
	{
		//AnimationHelper.WithVelocity(WishVelocity);
		//AnimationHelper.WithWishVelocity(WishVelocity);
		//AnimationHelper.IsNoclipping = true;
	}
}
