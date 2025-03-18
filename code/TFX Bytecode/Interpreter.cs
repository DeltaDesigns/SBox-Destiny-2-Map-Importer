using System;
using System.Threading.Tasks;
using Vec4 = System.Numerics.Vector4;

public class TfxBytecodeInterpreter
{
	public List<TfxData> Opcodes { get; set; }
	public Stack<Vec4> Stack { get; set; }
	public List<Vec4> Temp { get; set; }
	public string Light { get; set; } = "";

	private TfxData _curOp { get; set; }

	public TfxBytecodeInterpreter( List<TfxData> opcodes )
	{
		Opcodes = opcodes ?? new List<TfxData>();
		Stack = new( capacity: 64 );
		Temp = new( capacity: 16 );
	}

	private List<Vec4> StackPop( int pops )
	{
		if ( Stack.Count < pops )
			throw new Exception( $"Light {Light}: Not enough elements in the stack to pop. Op {_curOp.op} (Stack Count {Stack.Count} : Pops {pops})" );

		var poppedItems = new List<Vec4>();

		switch ( pops )
		{
			case 1:
				poppedItems.Add( Stack.Pop() );
				break;
			case 2:
				poppedItems.Add( Stack.Pop() );
				poppedItems.Add( Stack.Pop() );
				break;
			case 3:
				poppedItems.Add( Stack.Pop() );
				poppedItems.Add( Stack.Pop() );
				poppedItems.Add( Stack.Pop() );
				break;
			default:
				throw new Exception( $"Light {Light}: Cannot pop {pops} from stack. Op {_curOp.op}" );
		}

		poppedItems.Reverse();
		return poppedItems;
	}

	private void StackPush( Vec4 value )
	{
		if ( Stack.Count >= 64 )
			throw new Exception( $"Light {Light}: Stack is at capacity. Op {_curOp.op}" );

		Stack.Push( value );
	}

	private Vec4 StackTop()
	{
		if ( Stack.Count == 0 )
			throw new Exception( $"Light {Light}: Stack is empty. Op {_curOp.op}" );

		return Stack.Pop();
	}

	public async Task<Dictionary<int, Vec4>> Evaluate( Vector4[] constants, bool print = false )
	{
		Dictionary<int, Vec4> hlsl = new();
		await Sandbox.GameTask.RunInThreadAsync( () =>
		{
			try
			{
				if ( print ) Log.Info( $"--------Evaluating Bytecode:" );
				foreach ( var op in Opcodes )
				{
					if ( print ) Log.Info( $"{op.op} : {TfxBytecodeOp.TfxToString( op, constants )}" );
					_curOp = op;
					switch ( op.op )
					{
						case TfxBytecode.Add:
						case TfxBytecode.Add2:
							var add = StackPop( 2 );
							StackPush( add[0] + add[1] );
							break;

						case TfxBytecode.Subtract:
							var sub = StackPop( 2 );
							StackPush( sub[0] - sub[1] );
							break;

						case TfxBytecode.Multiply:
						case TfxBytecode.Multiply2:
							var mul = StackPop( 2 );
							StackPush( mul[0] * mul[1] );
							break;

						case TfxBytecode.Divide:
							var div = StackPop( 2 );
							StackPush( div[0] / div[1] );
							break;

						case TfxBytecode.IsZero:
							var isZero = StackTop();
							StackPush( new Vec4( isZero.X == 0 ? 1 : 0,
								isZero.Y == 0 ? 1 : 0,
								isZero.Z == 0 ? 1 : 0,
								isZero.W == 0 ? 1 : 0 ) );
							break;

						case TfxBytecode.Min:
							var min = StackPop( 2 );
							StackPush( Vec4.Min( min[0], min[1] ) );
							break;

						case TfxBytecode.Max:
							var max = StackPop( 2 );
							StackPush( Vec4.Max( max[0], max[1] ) );
							break;

						case TfxBytecode.LessThan: //I dont think I need to do < for each element?
							var lessThan = StackPop( 2 );
							StackPush( new Vec4( lessThan[0].X < lessThan[1].X ? 1 : 0,
								lessThan[0].Y < lessThan[1].Y ? 1 : 0,
								lessThan[0].Z < lessThan[1].Z ? 1 : 0,
								lessThan[0].W < lessThan[1].W ? 1 : 0 ) );

							break;

						case TfxBytecode.Dot:
							var dot = StackPop( 2 );
							StackPush( new Vec4( Vec4.Dot( dot[0], dot[1] ) ) );
							break;

						case TfxBytecode.Merge_1_3:
							var merge = StackPop( 2 );
							StackPush( new Vec4( merge[0].X, merge[1].X, merge[1].Y, merge[1].Z ) );
							break;

						case TfxBytecode.Merge_2_2:
							var merge2_2 = StackPop( 2 );
							StackPush( new Vec4( merge2_2[0].X, merge2_2[0].Y, merge2_2[1].X, merge2_2[1].Y ) );
							break;

						case TfxBytecode.Merge_3_1:
							var merge3_1 = StackPop( 2 );
							StackPush( new Vec4( merge3_1[0].X, merge3_1[0].Y, merge3_1[0].Z, merge3_1[1].X ) );
							break;

						case TfxBytecode.Cubic:
							var cubic = StackPop( 2 );
							StackPush( bytecode_op_cubic( cubic[0], cubic[1] ) );
							break;

						case TfxBytecode.Lerp:
							var lerp = StackPop( 3 );
							StackPush( lerp[1] + lerp[2] * (lerp[0] - lerp[1]) );
							break;

						case TfxBytecode.MultiplyAdd:
							var mulAdd = StackPop( 3 );
							StackPush( mulAdd[0] * mulAdd[1] + mulAdd[2] );
							break;

						case TfxBytecode.Clamp:
							var clamp = StackPop( 3 );
							StackPush( Vec4.Clamp( clamp[0], clamp[1], clamp[2] ) );
							break;

						case TfxBytecode.Abs:
							StackPush( Vec4.Abs( StackTop() ) );
							break;

						case TfxBytecode.Sign:
							var sign = StackTop();
							StackPush( new Vec4(
								Math.Sign( sign.X ),
								Math.Sign( sign.Y ),
								Math.Sign( sign.Z ),
								Math.Sign( sign.W )
							) );
							break;

						case TfxBytecode.Floor:
							var floor = StackTop();
							StackPush( new Vec4(
									MathF.Floor( floor.X ),
									MathF.Floor( floor.Y ),
									MathF.Floor( floor.Z ),
									MathF.Floor( floor.W )
								) );
							break;

						case TfxBytecode.Ceil:
							var ceil = StackTop();
							StackPush( new Vec4(
									MathF.Ceiling( ceil.X ),
									MathF.Ceiling( ceil.Y ),
									MathF.Ceiling( ceil.Z ),
									MathF.Ceiling( ceil.W )
								) );
							break;

						case TfxBytecode.Round:
							var round = StackTop();
							StackPush( new Vec4(
									MathF.Round( round.X ),
									MathF.Round( round.Y ),
									MathF.Round( round.Z ),
									MathF.Round( round.W )
								) );
							break;

						case TfxBytecode.Frac:
							var frac = StackTop();
							StackPush( new Vec4(
									frac.X - MathF.Truncate( frac.X ),
									frac.Y - MathF.Truncate( frac.Y ),
									frac.Z - MathF.Truncate( frac.Z ),
									frac.W - MathF.Truncate( frac.W )
								) );
							break;

						case TfxBytecode.Negate:
							StackPush( Vec4.Negate( StackTop() ) );
							break;

						case TfxBytecode.VecRotSin:
							StackPush( _trig_helper_vector_sin_rotations_estimate( StackTop() ) );
							break;

						case TfxBytecode.VecRotCos:
							StackPush( _trig_helper_vector_cos_rotations_estimate( StackTop() ) );
							break;

						case TfxBytecode.VecRotSinCos:
							StackPush( _trig_helper_vector_sin_cos_rotations_estimate( StackTop() ) );
							break;

						case TfxBytecode.PermuteAllX:
							StackPush( new Vec4( StackTop().X ) );
							break;

						case TfxBytecode.Permute:
							var fields = ((PermuteData)op.data).fields;
							var permute = StackTop();

							var s0 = (fields >> 6) & 0b11;
							var s1 = (fields >> 4) & 0b11;
							var s2 = (fields >> 2) & 0b11;
							var s3 = fields & 0b11;
							float[] array = new float[] { permute.X, permute.Y, permute.Z, permute.W };

							StackPush( new Vec4(
							array[s0],
							array[s1],
							array[s2],
							array[s3] ) );

							break;

						case TfxBytecode.Saturate:
							var saturate = StackTop();
							StackPush( Saturate( saturate ) );
							break;

						case TfxBytecode.Triangle:
							StackPush( bytecode_op_triangle( StackTop() ) );
							break;

						case TfxBytecode.Jitter:
							StackPush( bytecode_op_jitter( StackTop() ) );
							break;

						case TfxBytecode.Wander:
							StackPush( bytecode_op_wander( StackTop() ) );
							break;

						case TfxBytecode.Rand:
							StackPush( bytecode_op_rand( StackTop() ) );
							break;

						case TfxBytecode.RandSmooth:
							StackPush( bytecode_op_rand_smooth( StackTop() ) );
							break;

						case TfxBytecode.TransformVec4:
							StackPush( mul_vec4( StackPop( 5 ) ) );
							break;

						case TfxBytecode.PushConstantVec4:
							var vec = constants[((PushConstantVec4Data)op.data).constant_index];
							StackPush( vec );
							break;

						case TfxBytecode.LerpConstant:
							var t = StackTop();
							var a = constants[((LerpConstantData)op.data).constant_start];
							var b = constants[((LerpConstantData)op.data).constant_start + 1];

							StackPush( a + t * (b - a) );
							break;

						case TfxBytecode.Spline4Const:
							var X = StackTop();
							var threshold = constants[((Spline4ConstData)op.data).constant_index + 4];
							var C3 = constants[((Spline4ConstData)op.data).constant_index];
							var C2 = constants[((Spline4ConstData)op.data).constant_index + 1];
							var C1 = constants[((Spline4ConstData)op.data).constant_index + 2];
							var C0 = constants[((Spline4ConstData)op.data).constant_index + 3];

							StackPush( bytecode_op_spline4_const( X, C3, C2, C1, C0, threshold ) );
							break;

						case TfxBytecode.Spline8Const:
							var s8c_index = ((Spline8ConstData)op.data).constant_index;
							var X_1 = StackTop();
							var C_thresholds = constants[s8c_index + 8];
							var D_thresholds = constants[s8c_index + 9];
							var C3_1 = constants[s8c_index];
							var C2_1 = constants[s8c_index + 1];
							var C1_1 = constants[s8c_index + 2];
							var C0_1 = constants[s8c_index + 3];
							var D3 = constants[s8c_index + 4];
							var D2 = constants[s8c_index + 5];
							var D1 = constants[s8c_index + 6];
							var D0 = constants[s8c_index + 7];

							StackPush( bytecode_op_spline8_const( X_1, C3_1, C2_1, C1_1, C0_1, D3, D2, D1, D0, C_thresholds, D_thresholds ) );
							break;

						case TfxBytecode.Gradient4Const:
							var g4c_index = ((Gradient4ConstData)op.data).constant_index;
							var X_g4c = StackTop();
							var BaseColor = constants[g4c_index];
							var Cred = constants[g4c_index + 1];
							var Cgreen = constants[g4c_index + 2];
							var Cblue = constants[g4c_index + 3];
							var Calpha = constants[g4c_index + 4];
							var Cthresholds = constants[g4c_index + 5];

							StackPush( bytecode_op_gradient4_const( X_g4c, BaseColor, Cred, Cgreen, Cblue, Calpha, Cthresholds ) );
							break;

						case TfxBytecode.Gradient8Const: // A massive unknown function with a 12 inputs, maybe this is Gradient8Const? (idk if that exists)
							var g8c_index = ((Gradient8ConstData)op.data).constant_index;
							var g8c_X1 = StackTop();
							var g8c_BaseColor = constants[g8c_index];
							var g8c_Cred = constants[g8c_index + 1];
							var g8c_Cgreen = constants[g8c_index + 2];
							var g8c_Cblue = constants[g8c_index + 3];
							var g8c_Calpha = constants[g8c_index + 4];
							var g8c_Dred = constants[g8c_index + 5];
							var g8c_Dgreen = constants[g8c_index + 6];
							var g8c_Dblue = constants[g8c_index + 7];
							var g8c_Dalpha = constants[g8c_index + 8];
							var g8c_Cthresholds = constants[g8c_index + 9];
							var g8c_Dthresholds = constants[g8c_index + 10];

							StackPush( bytecode_op_gradient8_const( g8c_X1, g8c_BaseColor, g8c_Cred, g8c_Cgreen, g8c_Cblue, g8c_Calpha, g8c_Dred, g8c_Dgreen, g8c_Dblue, g8c_Dalpha, g8c_Cthresholds, g8c_Dthresholds ) );
							break;

						case TfxBytecode.PushExternInputFloat:
							var v = GetExternFloat( ((PushExternInputFloatData)op.data).extern_, ((PushExternInputFloatData)op.data).element );
							StackPush( v );
							break;

						case TfxBytecode.PushExternInputVec4:
							var PushExternInputVec4 = GetExternVec4( ((PushExternInputVec4Data)op.data).extern_, ((PushExternInputVec4Data)op.data).element );
							StackPush( PushExternInputVec4 );
							break;

						case TfxBytecode.PushExternInputMat4:
							//var Mat4 = Matrix4x4.Identity;
							StackPush( new Vec4( 1f, 0f, 0f, 0f ) );
							StackPush( new Vec4( 0f, 1f, 0f, 0f ) );
							StackPush( new Vec4( 0f, 0f, 1f, 0f ) );
							StackPush( new Vec4( 0f, 0f, 0f, 1f ) );
							break;

						case TfxBytecode.Unk42:
						case TfxBytecode.Unk4c:
							StackPush( Vec4.One );
							break;
						case TfxBytecode.Unk50:
							StackPush( Vec4.Zero );
							break;
						case TfxBytecode.Unk2c:
						case TfxBytecode.Unk49:
						case TfxBytecode.Unk51:
							_ = StackPop( 1 );
							break;
						case TfxBytecode.Unk2d:
							_ = StackPop( 4 );
							break;
						case TfxBytecode.Unk14:
							_ = StackPop( 2 );
							break;

						case TfxBytecode.PushGlobalChannelVector:
							var global_channel = GlobalChannelDefaults.GlobalChannels[((PushGlobalChannelVectorData)op.data).unk1];
							StackPush( global_channel );
							break;

						case TfxBytecode.PushTexDimensions:
							StackPush( Vec4.One );
							break;

						case TfxBytecode.PushTexTileParams:
							StackPush( Vec4.One );
							break;

						case TfxBytecode.PushTexTileCount:
							StackPush( Vec4.One );
							break;

						case TfxBytecode.PushExternInputTextureView:
						case TfxBytecode.PushExternInputUav:
						case TfxBytecode.SetShaderTexture:
						case TfxBytecode.SetShaderSampler:
						case TfxBytecode.PushSampler:
							break;

						case TfxBytecode.PushObjectChannelVector:
							StackPush( new Vec4( 1f ) );
							break;

						case TfxBytecode.PushFromOutput:
							StackPush( hlsl[((PushFromOutputData)op.data).element] );
							break;

						case TfxBytecode.PopOutput:
							//Temp.AddRange(Stack);

							if ( print )
								Log.Info( $"----Output Stack Count: {Stack.Count}" );

							if ( Stack.Count == 0 ) //Shouldnt happen							
								return;
							else
								hlsl.TryAdd( ((PopOutputData)op.data).slot, StackTop() );

							Stack.Clear(); //Does this matter?
							break;

						case TfxBytecode.PopOutputMat4: //uhhhhh, im 100% doing this wrong
							var PopOutputMat4 = StackPop( 4 );
							var Mat4_1 = PopOutputMat4[0];
							var Mat4_2 = PopOutputMat4[1];
							var Mat4_3 = PopOutputMat4[2];
							var Mat4_4 = PopOutputMat4[3];

							hlsl.TryAdd( ((PopOutputMat4Data)op.data).slot, Mat4_1 );
							hlsl.TryAdd( ((PopOutputMat4Data)op.data).slot + 1, Mat4_2 );
							hlsl.TryAdd( ((PopOutputMat4Data)op.data).slot + 2, Mat4_3 );
							hlsl.TryAdd( ((PopOutputMat4Data)op.data).slot + 3, Mat4_4 );
							Stack.Clear();
							break;

						case TfxBytecode.PushTemp:
							var PushTemp = ((PushTempData)op.data).slot;
							StackPush( Temp[PushTemp] );
							break;

						case TfxBytecode.PopTemp:
							var PopTemp = ((PopTempData)op.data).slot;
							var PopTemp_v = StackTop();
							Temp.Insert( PopTemp, PopTemp_v );
							break;

						default:
							Log.Error( $"Light {Light}: Not Implemented: {op.op}" );
							break;

					}
				}
			}
			catch ( Exception e )
			{
				Log.Error( $"Light {Light}: {e.Message}" );
			}
		} );
		return hlsl;
	}

	private Vec4 GetExternFloat( TfxExtern extern_, byte element )
	{
		switch ( extern_ )
		{
			case TfxExtern.Frame:
				switch ( element * 0x4 )
				{
					case 0x0:
						return new Vec4( RealTime.Now ); // game_time
					case 0x4:
						return new Vec4( RealTime.Now ); // render_time
					case 0xC:
						return new Vec4( 1f ); // Unk
					case 0x10:
						return new Vec4( 1f ); // Unk
					case 0x14:
						return new Vec4( Time.Delta ); // delta_game_time
					case 0x1C:
						return new Vec4( 1f ); // exposure_scale
					default:
						Log.Error( $"Unsupported element {element * 0x4} (0x{(element * 0x4):X}) for extern {extern_}" );
						return new Vec4( 1f );
				}
			default:
				Log.Error( $"Unsupported extern {extern_}[{element}]" );
				return new Vec4( 1f );
		}
	}

	private Vec4 GetExternVec4( TfxExtern extern_, byte element )
	{
		switch ( extern_ )
		{
			case TfxExtern.Frame:
				switch ( element )
				{
					case 26:
						return new Vec4( 0f );
					case 27:
						return new Vec4( 1f );
					default:
						Log.Error( $"Unsupported element {element} for extern {extern_}" );
						return new Vec4( 0f );
				}
			case TfxExtern.Atmosphere:
				switch ( element )
				{
					case 7:
						return new Vec4( 1f );
					default:
						Log.Error( $"Unsupported element {element} for extern {extern_}" );
						return new Vec4( 0f );
				}
			default:
				Log.Error( $"Unsupported extern {extern_}[{element}]" );
				return new Vec4( 1f );
		}
	}

	private Vec4 bytecode_op_spline4_const(
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
	private Vec4 bytecode_op_spline8_const(
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

	private Vec4 bytecode_op_gradient4_const(
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

	private Vec4 bytecode_op_gradient8_const(
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

	public bool GreaterEqual( Vec4 vec4, float x )
	{
		return (vec4.X >= x && vec4.Y >= x && vec4.Z >= x && vec4.W >= x);
	}

	public bool NotEqual( Vec4 vec4, float x )
	{
		return (vec4.X != x && vec4.Y != x && vec4.Z != x && vec4.W != x);
	}

	private Vec4 _fake_bitwise_ops_fake_xor( Vec4 a, Vec4 b )
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

	private Vec4 bytecode_op_triangle( Vec4 x )
	{
		var wrapped = x - Round( x ); // wrap to [-0.5, 0.5] range
		var abs_wrap = Vec4.Abs( wrapped ); // abs turns into triangle wave between [0, 0.5]

		return abs_wrap * 2.0f; // scale to [0, 1] range
	}

	private Vec4 bytecode_op_jitter( Vec4 x )
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

	private Vec4 bytecode_op_wander( Vec4 x )
	{
		var rot0 = new Vec4( x.X ) * new Vec4( 4.08f, 1.02f, 3.0f / 5.37f, 3.0f / 9.67f ) + new Vec4( 0.92f, 0.33f, 0.26f, 0.54f );
		var rot1 = new Vec4( x.X ) * new Vec4( 1.83f, 3.09f, 0.39f, 0.87f ) + new Vec4( 0.12f, 0.37f, 0.16f, 0.79f );
		var sines0 = _trig_helper_vector_pseudo_sin_rotations( rot0 );
		var sines1 = _trig_helper_vector_pseudo_sin_rotations( rot1 ) * new Vec4( 0.02f, 0.02f, 0.28f, 0.28f );
		var wander_result = 0.5f + Vec4.Dot( sines0, sines1 );

		return new Vec4( wander_result );
	}

	private Vec4 bytecode_op_rand( Vec4 x )
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

	private Vec4 bytecode_op_rand_smooth( Vec4 x )
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

	private Vec4 bytecode_op_cubic(
		Vec4 X,
		Vec4 coefficients )
	{

		Vec4 high = new Vec4( coefficients.X ) * X + new Vec4( coefficients.Y );
		Vec4 low = new Vec4( coefficients.Z ) * X + new Vec4( coefficients.W );
		Vec4 X2 = X * X;
		Vec4 cubic_result = high * X2 + low;

		return cubic_result;
	}

	private Vec4 mul_vec4( List<Vec4> TransformVec4 ) //probably wrong
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

	private Vec4 _trig_helper_vector_sin_rotations_estimate_clamped( Vec4 a )
	{
		var y = a * (-16.0f * Vec4.Abs( a ) + new Vec4( 8.0f ));
		return y * (0.225f * Vec4.Abs( y ) + new Vec4( 0.775f ));
	}

	private Vec4 _trig_helper_vector_sin_rotations_estimate( Vec4 a )
	{
		var w = a - Round( a );
		return _trig_helper_vector_sin_rotations_estimate_clamped( w );
	}

	private Vec4 _trig_helper_vector_cos_rotations_estimate( Vec4 a )
	{
		return _trig_helper_vector_sin_rotations_estimate( a + new Vec4( 0.25f ) );
	}

	private Vec4 _trig_helper_vector_sin_cos_rotations_estimate( Vec4 a )
	{
		return _trig_helper_vector_sin_rotations_estimate( a + new Vec4( 0.0f, 0.25f, 0.0f, 0.25f ) );
	}

	//pseudo
	private Vec4 _trig_helper_vector_pseudo_sin_rotations( Vec4 a )
	{
		var w = a - Round( a ); // wrap to [-0.5, 0.5] range
		return _trig_helper_vector_pseudo_sin_rotations_clamped( w );
	}

	private Vec4 _trig_helper_vector_pseudo_sin_rotations_clamped( Vec4 x )
	{
		var wrapped = x - Round( x ); // wrap to [-0.5, 0.5] range
		var abs_wrap = Vec4.Abs( wrapped ); // abs turns into triangle wave between [0, 0.5]

		return abs_wrap * 2.0f; // scale to [0, 1] range
	}

	private Vec4 Round( Vec4 x )
	{
		return new Vec4( MathF.Round( x.X ), MathF.Round( x.Y ), MathF.Round( x.Z ), MathF.Round( x.W ) );
	}

	private float Fract( float x )
	{
		return x - MathF.Truncate( x );
	}

	private float lerp( float start, float end, float t )
	{
		return start + (end - start) * t;
	}

	public Vec4 Step( Vec4 edge, Vec4 value )
	{
		return new Vec4(
			value.X >= edge.X ? 1f : 0f,
			value.Y >= edge.Y ? 1f : 0f,
			value.Z >= edge.Z ? 1f : 0f,
			value.W >= edge.W ? 1f : 0f
		);
	}

	public Vec4 Saturate( Vec4 saturate )
	{
		return new Vec4(
			Math.Clamp( saturate.X, 0f, 1f ),
			Math.Clamp( saturate.Y, 0f, 1f ),
			Math.Clamp( saturate.Z, 0f, 1f ),
			Math.Clamp( saturate.W, 0f, 1f )
		);
	}
}
