using System.Linq;

namespace Sandbox.Movement;

/// <summary>
/// The character is not bound by physics.
/// </summary>
[Icon( "self_improvement" ), Group( "Movement" ), Title( "MoveMode - Noclip" )]
public class MoveModeNoclip : MoveMode
{
	[Property] public int Priority { get; set; } = 100;

	/// <summary>
	/// Button to toggle noclipping.
	/// </summary>
	[Property, InputAction]
	public string NoclipButton { get; set; } = "noclip";

	/// <summary>
	/// Command to toggle noclipping.
	/// </summary>
	[ConCmd( "noclip", Help = "Toggles noclipping." )]
	public static void NoclipCommand()
	{
		var caller = Game.ActiveScene.GetAllComponents<MoveModeNoclip>()
			.FirstOrDefault( x => x.Network.Owner == Rpc.Caller );

		if ( !caller.IsValid() ) return;

		caller.IsNoclipping = !caller.IsNoclipping;
	}

	/// <summary>
	/// Set to true when entering a noclip <see cref="MoveMode"/>.
	/// </summary>
	public bool IsNoclipping { get; set; }

	protected override void OnFixedUpdate()
	{
		if ( IsProxy || !Input.Pressed( NoclipButton ) ) return;

		IsNoclipping = !IsNoclipping;
	}

	public override int Score( PlayerController controller )
	{
		if ( IsNoclipping ) return Priority;
		return -100;
	}

	public override void AddVelocity()
	{
		Controller.Body.Velocity = Controller.WishVelocity * 5;
	}

	public override Vector3 UpdateMove( Rotation eyes, Vector3 input )
	{
		var velocity = eyes * input;

		if ( Input.Down( "Jump" ) ) velocity.z += 1;
		if ( Input.Down( "Duck" ) ) velocity.z -= 1;

		bool run = Input.Down( Controller.AltMoveButton );
		velocity *= run ? Controller.RunSpeed : Controller.WalkSpeed;

		return velocity;
	}

	public override void OnModeBegin()
	{
		Controller.Body.MotionEnabled = false;

		Controller?.Renderer?.Set( "b_noclip", true );
	}

	public override void OnModeEnd( MoveMode next )
	{
		Controller.Body.MotionEnabled = true;

		Controller?.Renderer?.Set( "b_noclip", false );
	}
}
