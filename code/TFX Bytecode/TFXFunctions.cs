using System;
using Vec4 = System.Numerics.Vector4;

public static class TFXFunctions
{
	public static Vec4 bytecode_op_spline4_const(
		Vec4 X,
		Vec4 C3,
		Vec4 C2,
		Vec4 C1,
		Vec4 C0,
		Vec4 thresholds )
	{
		Vec4 high = C3 * X + C2;
		Vec4 low = C1 * X + C0;
		Vec4 X2 = X * X;
		Vec4 evaluated_spline = high * X2 + low;

		Vec4 threshold_mask = Step( thresholds, X );
		var a = _fake_bitwise_ops_fake_xor( threshold_mask, new Vec4( threshold_mask.Y, threshold_mask.Z, threshold_mask.W, threshold_mask.W ) );
		Vec4 channel_mask = new Vec4( a.X, a.Y, a.Z, threshold_mask.W );
		Vec4 spline_result_in_4 = evaluated_spline * channel_mask;
		float spline_result = spline_result_in_4.X + spline_result_in_4.Y + spline_result_in_4.Z + spline_result_in_4.W;

		return new( spline_result );
	}

	// evals a cubic polynomial across eight channels with estrin form
	public static Vec4 bytecode_op_spline8_const(
		Vec4 X,
		Vec4 C3,
		Vec4 C2,
		Vec4 C1,
		Vec4 C0,
		Vec4 D3,
		Vec4 D2,
		Vec4 D1,
		Vec4 D0,
		Vec4 C_thresholds,
		Vec4 D_thresholds )
	{
		Vec4 C_high = C3 * X + C2;
		Vec4 C_low = C1 * X + C0;
		Vec4 D_high = D3 * X + D2;
		Vec4 D_low = D1 * X + D0;
		Vec4 X2 = X * X;
		Vec4 C_evaluated_spline = C_high * X2 + C_low;
		Vec4 D_evaluated_spline = D_high * X2 + D_low;

		Vec4 C_threshold_mask = Step( C_thresholds, X );
		Vec4 D_threshold_mask = Step( D_thresholds, X );

		var a = new Vec4( C_threshold_mask.Y, C_threshold_mask.Z, C_threshold_mask.W, C_threshold_mask.W );
		var b = _fake_bitwise_ops_fake_xor( C_threshold_mask, a );
		Vec4 C_channel_mask = new Vec4( b.X, b.Y, b.Z, C_threshold_mask.W );

		a = new Vec4( D_threshold_mask.Y, D_threshold_mask.Z, D_threshold_mask.W, D_threshold_mask.W );
		b = _fake_bitwise_ops_fake_xor( D_threshold_mask, a );
		Vec4 D_channel_mask = new Vec4( b.X, b.Y, b.Z, D_threshold_mask.W );

		Vec4 C_spline_result_in_4 = C_evaluated_spline * C_channel_mask;
		Vec4 D_spline_result_in_4 = D_evaluated_spline * D_channel_mask;
		float C_spline_result = C_spline_result_in_4.X + C_spline_result_in_4.Y + C_spline_result_in_4.Z + C_spline_result_in_4.W;
		float D_spline_result = D_spline_result_in_4.X + D_spline_result_in_4.Y + D_spline_result_in_4.Z + D_spline_result_in_4.W;
		float spline_result = D_threshold_mask.X == 1 ? D_spline_result : C_spline_result;

		return new Vec4( spline_result );
	}

	public static Vec4 bytecode_op_gradient4_const(
		Vec4 X,
		Vec4 BaseColor,
		Vec4 Cred,
		Vec4 Cgreen,
		Vec4 Cblue,
		Vec4 Calpha,
		Vec4 Cthresholds )
	{
		// Compute the weighting of each gradient delta based upon the X position of evaluation.
		Vec4 Coffsets_from_x = X - Cthresholds;
		Vec4 Csegment_interval = new Vec4( Cthresholds.Y, Cthresholds.Z, Cthresholds.W, 1.0f ) - Cthresholds;
		Vec4 Csafe_division = GreaterEqual( Coffsets_from_x, 0.0f ) ? new Vec4( 1.0f, 1.0f, 1.0f, 1.0f ) : new Vec4( 0.0f, 0.0f, 0.0f, 0.0f );
		Vec4 Cdivision = NotEqual( Csegment_interval, 0.0f ) ? (Coffsets_from_x / Csegment_interval) : Csafe_division;
		Vec4 Cpercentages = Saturate( Cdivision );

		// Compute the influence that each of the colors will contribute to the final color.
		Vec4 Xinfluence = Cred * Cpercentages;
		Vec4 Yinfluence = Cgreen * Cpercentages;
		Vec4 Zinfluence = Cblue * Cpercentages;
		Vec4 Winfluence = Calpha * Cpercentages;

		// Add the colors into the base color.
		Vec4 gradient_result = BaseColor + new Vec4( Vec4.Dot( new Vec4( 1.0f ), Xinfluence ),
													 Vec4.Dot( new Vec4( 1.0f ), Yinfluence ),
													 Vec4.Dot( new Vec4( 1.0f ), Zinfluence ),
													 Vec4.Dot( new Vec4( 1.0f ), Winfluence ) );
		return gradient_result;
	}

	public static Vec4 bytecode_op_gradient8_const(
		Vec4 X,
		Vec4 BaseColor,
		Vec4 Cred,
		Vec4 Cgreen,
		Vec4 Cblue,
		Vec4 Calpha,
		Vec4 Dred,
		Vec4 Dgreen,
		Vec4 Dblue,
		Vec4 Dalpha,
		Vec4 Cthresholds,
		Vec4 Dthresholds )
	{
		// Compute the weighting of each gradient delta based upon the X position of evaluation.
		Vec4 Coffsets_from_x = X - Cthresholds;
		Vec4 Csegment_interval = new Vec4( Cthresholds.Y, Cthresholds.Z, Cthresholds.W, 1.0f ) - Cthresholds;
		Vec4 Csafe_division = GreaterEqual( Coffsets_from_x, 0.0f ) ? new Vec4( 1.0f, 1.0f, 1.0f, 1.0f ) : new Vec4( 0.0f, 0.0f, 0.0f, 0.0f );
		Vec4 Cdivision = NotEqual( Csegment_interval, 0.0f ) ? (Coffsets_from_x / Csegment_interval) : Csafe_division;
		Vec4 Cpercentages = Saturate( Cdivision );

		Vec4 Doffsets_from_x = X - Dthresholds;
		Vec4 Dsegment_interval = new Vec4( Dthresholds.Y, Dthresholds.Z, Dthresholds.W, 1.0f ) - Dthresholds;
		Vec4 Dsafe_division = GreaterEqual( Doffsets_from_x, 0.0f ) ? new Vec4( 1.0f, 1.0f, 1.0f, 1.0f ) : new Vec4( 0.0f, 0.0f, 0.0f, 0.0f );
		Vec4 Ddivision = NotEqual( Dsegment_interval, 0.0f ) ? (Doffsets_from_x / Dsegment_interval) : Dsafe_division;
		Vec4 Dpercentages = Saturate( Ddivision );

		// Compute the influence that each of the colors will contribute to the final color.
		Vec4 Xinfluence = (Cred * Cpercentages) + (Dred * Dpercentages);
		Vec4 Yinfluence = (Cgreen * Cpercentages) + (Dgreen * Dpercentages);
		Vec4 Zinfluence = (Cblue * Cpercentages) + (Dblue * Dpercentages);
		Vec4 Winfluence = (Calpha * Cpercentages) + (Dalpha * Dpercentages);

		// Add the colors into the base color.
		Vec4 gradient_result = BaseColor + new Vec4( Vec4.Dot( new Vec4( 1.0f ), Xinfluence ),
													 Vec4.Dot( new Vec4( 1.0f ), Yinfluence ),
													 Vec4.Dot( new Vec4( 1.0f ), Zinfluence ),
													 Vec4.Dot( new Vec4( 1.0f ), Winfluence ) );
		return gradient_result;
	}

	public static bool GreaterEqual( Vec4 vec4, float x )
	{
		return (vec4.X >= x && vec4.Y >= x && vec4.Z >= x && vec4.W >= x);
	}

	public static bool NotEqual( Vec4 vec4, float x )
	{
		return (vec4.X != x && vec4.Y != x && vec4.Z != x && vec4.W != x);
	}

	public static Vec4 _fake_bitwise_ops_fake_xor( Vec4 a, Vec4 b )
	{
		return Fmod( a + b, 2f );
	}

	public static Vec4 Fmod( Vec4 value, float modulus )
	{
		return new Vec4(
			MathF.IEEERemainder( value.X, modulus ),
			MathF.IEEERemainder( value.Y, modulus ),
			MathF.IEEERemainder( value.Z, modulus ),
			MathF.IEEERemainder( value.W, modulus )
		);
	}

	public static Vec4 bytecode_op_triangle( Vec4 x )
	{
		var wrapped = x - Round( x ); // wrap to [-0.5, 0.5] range
		var abs_wrap = Vec4.Abs( wrapped ); // abs turns into triangle wave between [0, 0.5]

		return abs_wrap * 2.0f; // scale to [0, 1] range
	}

	public static Vec4 bytecode_op_jitter( Vec4 x )
	{
		var rotations = new Vec4( x.X ) * new Vec4( 4.67f, 2.99f, 1.08f, 1.35f ) + new Vec4( 0.52f, 0.37f, 0.16f, 0.79f );

		// optimized scaled-sum-of-sines
		var a = rotations - Round( rotations ); // wrap to [-0.5, 0.5] range
		var ma = Vec4.Abs( a ) * -16.0f + new Vec4( 8.0f );
		var sa = a * 0.25f;
		var v = Vec4.Dot( sa, ma ) + 0.5f;

		// hermite smooth interpolation (3*v^2 - 2*v^3)
		var v2 = v * v;
		var jitter_result = (-2.0f * v + 3.0f) * v2;

		return new Vec4( jitter_result );
	}

	public static Vec4 bytecode_op_wander( Vec4 x )
	{
		var rot0 = new Vec4( x.X ) * new Vec4( 4.08f, 1.02f, 3.0f / 5.37f, 3.0f / 9.67f ) + new Vec4( 0.92f, 0.33f, 0.26f, 0.54f );
		var rot1 = new Vec4( x.X ) * new Vec4( 1.83f, 3.09f, 0.39f, 0.87f ) + new Vec4( 0.12f, 0.37f, 0.16f, 0.79f );
		var sines0 = _trig_helper_vector_pseudo_sin_rotations( rot0 );
		var sines1 = _trig_helper_vector_pseudo_sin_rotations( rot1 ) * new Vec4( 0.02f, 0.02f, 0.28f, 0.28f );
		var wander_result = 0.5f + Vec4.Dot( sines0, sines1 );

		return new Vec4( wander_result );
	}

	public static Vec4 bytecode_op_rand( Vec4 x )
	{
		// these magic numbers are 1/(prime/1000000)
		var v0 = MathF.Floor( x.X );
		var val0 = Vec4.Dot( new Vec4( v0 ), new Vec4(
			1.0f / 1.043501f,
			1.0f / 0.794471f,
			1.0f / 0.113777f,
			1.0f / 0.015101f ) );

		val0 = val0 - MathF.Truncate( val0 );

		//			val0=	bbs(val0);		// Blum-Blum-Shub randomimzer
		val0 = val0 * val0 * 251.0f;
		val0 = val0 - MathF.Truncate( val0 );

		return new Vec4( val0 );
	}

	public static Vec4 bytecode_op_rand_smooth( Vec4 x )
	{
		var v = x.X;
		var v0 = MathF.Round( v );
		var v1 = v0 + 1.0f;
		var f = v - v0;
		var f2 = f * f;

		// hermite smooth interpolation (3*f^2 - 2*f^3)
		var smooth_f = (-2.0f * f + 3.0f) * f2;

		// these magic numbers are 1/(prime/1000000)
		var val0 = Vec4.Dot( new Vec4( v0 ), new Vec4(
			1.0f / 1.043501f,
			1.0f / 0.794471f,
			1.0f / 0.113777f,
			1.0f / 0.015101f ) );

		var val1 = Vec4.Dot( new Vec4( v1 ), new Vec4(
			1.0f / 1.043501f,
			1.0f / 0.794471f,
			1.0f / 0.113777f,
			1.0f / 0.015101f ) );


		val0 = Fract( val0 );
		val1 = Fract( val1 );

		//			val0=	bbs(val0);		// Blum-Blum-Shub randomimzer
		val0 = val0 * val0 * 251.0f;
		val0 = Fract( val0 );

		//			val10=	bbs(val1);		// Blum-Blum-Shub randomimzer
		val1 = val1 * val1 * 251.0f;
		val1 = Fract( val1 );

		var rand_smooth_result = lerp( val0, val1, smooth_f );

		return new( rand_smooth_result );
	}

	public static Vec4 bytecode_op_cubic(
		Vec4 X,
		Vec4 coefficients )
	{

		Vec4 high = new Vec4( coefficients.X ) * X + new Vec4( coefficients.Y );
		Vec4 low = new Vec4( coefficients.Z ) * X + new Vec4( coefficients.W );
		Vec4 X2 = X * X;
		Vec4 cubic_result = high * X2 + low;

		return cubic_result;
	}

	public static Vec4 mul_vec4( List<Vec4> TransformVec4 ) //probably wrong
	{
		var x_axis = TransformVec4[0];
		var y_axis = TransformVec4[1];
		var z_axis = TransformVec4[2];
		var w_axis = TransformVec4[3];
		var value = TransformVec4[4];

		var res = x_axis * new Vec4( value.X );  //x_axis.mul(rhs.xxxx());

		res = (res + (y_axis * new Vec4( value.Y ))); //res = res.add(self.y_axis.mul(rhs.yyyy()));
		res = (res + (z_axis * new Vec4( value.Z ))); //res = res.add(self.z_axis.mul(rhs.zzzz()));
		res = (res + (w_axis * new Vec4( value.W ))); //res = res.add(self.w_axis.mul(rhs.wwww()));

		return res;
	}

	public static Vec4 _trig_helper_vector_sin_rotations_estimate_clamped( Vec4 a )
	{
		var y = a * (-16.0f * Vec4.Abs( a ) + new Vec4( 8.0f ));
		return y * (0.225f * Vec4.Abs( y ) + new Vec4( 0.775f ));
	}

	public static Vec4 _trig_helper_vector_sin_rotations_estimate( Vec4 a )
	{
		var w = a - Round( a );
		return _trig_helper_vector_sin_rotations_estimate_clamped( w );
	}

	public static Vec4 _trig_helper_vector_cos_rotations_estimate( Vec4 a )
	{
		return _trig_helper_vector_sin_rotations_estimate( a + new Vec4( 0.25f ) );
	}

	public static Vec4 _trig_helper_vector_sin_cos_rotations_estimate( Vec4 a )
	{
		return _trig_helper_vector_sin_rotations_estimate( a + new Vec4( 0.0f, 0.25f, 0.0f, 0.25f ) );
	}

	//pseudo
	public static Vec4 _trig_helper_vector_pseudo_sin_rotations( Vec4 a )
	{
		var w = a - Round( a ); // wrap to [-0.5, 0.5] range
		return _trig_helper_vector_pseudo_sin_rotations_clamped( w );
	}

	public static Vec4 _trig_helper_vector_pseudo_sin_rotations_clamped( Vec4 x )
	{
		var wrapped = x - Round( x ); // wrap to [-0.5, 0.5] range
		var abs_wrap = Vec4.Abs( wrapped ); // abs turns into triangle wave between [0, 0.5]

		return abs_wrap * 2.0f; // scale to [0, 1] range
	}

	public static Vec4 Round( Vec4 x )
	{
		return new Vec4( MathF.Round( x.X ), MathF.Round( x.Y ), MathF.Round( x.Z ), MathF.Round( x.W ) );
	}

	public static float Fract( float x )
	{
		return x - MathF.Truncate( x );
	}

	public static float lerp( float start, float end, float t )
	{
		return start + (end - start) * t;
	}

	public static Vec4 Step( Vec4 edge, Vec4 value )
	{
		return new Vec4(
			value.X >= edge.X ? 1f : 0f,
			value.Y >= edge.Y ? 1f : 0f,
			value.Z >= edge.Z ? 1f : 0f,
			value.W >= edge.W ? 1f : 0f
		);
	}

	public static Vec4 Saturate( Vec4 saturate )
	{
		return new Vec4(
			Math.Clamp( saturate.X, 0f, 1f ),
			Math.Clamp( saturate.Y, 0f, 1f ),
			Math.Clamp( saturate.Z, 0f, 1f ),
			Math.Clamp( saturate.W, 0f, 1f )
		);
	}
}
