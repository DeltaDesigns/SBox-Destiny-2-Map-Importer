using System;
using System.Threading.Tasks;
using Vec4 = System.Numerics.Vector4;

public class TfxBytecodeInterpreter
{
	public List<TfxData> Opcodes { get; set; }
	public Stack<Vec4> Stack { get; set; }
	public List<Vec4> Temp { get; set; }
	public string Name { get; set; } = "";

	private TfxData _curOp { get; set; }

	private GlobalChannelsController _globalChannels;
	private GlobalChannelsController GlobalChannels
	{
		get
		{
			if ( _globalChannels == null )
				_globalChannels = GlobalChannelsController.Get();

			return _globalChannels;
		}
		set
		{
			_globalChannels = value;
		}
	}

	public TfxBytecodeInterpreter( List<TfxData> opcodes )
	{
		Opcodes = opcodes ?? new List<TfxData>();
		Stack = new( capacity: 64 );
		Temp = new( capacity: 16 );
	}

	private List<Vec4> StackPop( int pops )
	{
		if ( Stack.Count < pops )
			throw new Exception( $"{Name}: Not enough elements in the stack to pop. Op {_curOp.op} (Stack Count {Stack.Count} : Pops {pops})" );

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
				throw new Exception( $"{Name}: Cannot pop {pops} from stack. Op {_curOp.op}" );
		}

		poppedItems.Reverse();
		return poppedItems;
	}

	private void StackPush( Vec4 value )
	{
		if ( Stack.Count >= 64 )
			throw new Exception( $"{Name}: Stack is at capacity. Op {_curOp.op}" );

		Stack.Push( value );
	}

	private Vec4 StackTop()
	{
		if ( Stack.Count == 0 )
			throw new Exception( $"{Name}: Stack is empty. Op {_curOp.op}" );

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
							StackPush( TFXFunctions.bytecode_op_cubic( cubic[0], cubic[1] ) );
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
							StackPush( TFXFunctions._trig_helper_vector_sin_rotations_estimate( StackTop() ) );
							break;

						case TfxBytecode.VecRotCos:
							StackPush( TFXFunctions._trig_helper_vector_cos_rotations_estimate( StackTop() ) );
							break;

						case TfxBytecode.VecRotSinCos:
							StackPush( TFXFunctions._trig_helper_vector_sin_cos_rotations_estimate( StackTop() ) );
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
							StackPush( TFXFunctions.Saturate( saturate ) );
							break;

						case TfxBytecode.Triangle:
							StackPush( TFXFunctions.bytecode_op_triangle( StackTop() ) );
							break;

						case TfxBytecode.Jitter:
							StackPush( TFXFunctions.bytecode_op_jitter( StackTop() ) );
							break;

						case TfxBytecode.Wander:
							StackPush( TFXFunctions.bytecode_op_wander( StackTop() ) );
							break;

						case TfxBytecode.Rand:
							StackPush( TFXFunctions.bytecode_op_rand( StackTop() ) );
							break;

						case TfxBytecode.RandSmooth:
							StackPush( TFXFunctions.bytecode_op_rand_smooth( StackTop() ) );
							break;

						case TfxBytecode.TransformVec4:
							StackPush( TFXFunctions.mul_vec4( StackPop( 5 ) ) );
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

							StackPush( TFXFunctions.bytecode_op_spline4_const( X, C3, C2, C1, C0, threshold ) );
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

							StackPush( TFXFunctions.bytecode_op_spline8_const( X_1, C3_1, C2_1, C1_1, C0_1, D3, D2, D1, D0, C_thresholds, D_thresholds ) );
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

							StackPush( TFXFunctions.bytecode_op_gradient4_const( X_g4c, BaseColor, Cred, Cgreen, Cblue, Calpha, Cthresholds ) );
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

							StackPush( TFXFunctions.bytecode_op_gradient8_const( g8c_X1, g8c_BaseColor, g8c_Cred, g8c_Cgreen, g8c_Cblue, g8c_Calpha, g8c_Dred, g8c_Dgreen, g8c_Dblue, g8c_Dalpha, g8c_Cthresholds, g8c_Dthresholds ) );
							break;

						case TfxBytecode.PushExternInputFloat:
							if ( op.type == TfxBytecodeOp.BytecodeType.Sequencer ) // TODO
							{
								//if ( Name.Contains( "sun_glow_color" ) )
								//	Log.Info( $"{Name}: {GlobalChannels.MiscValues[((PushExternInputFloatData)op.data).element]}" );
								StackPush( GlobalChannels.MiscValues[((PushExternInputFloatData)op.data).element] );
								//StackPush( GlobalChannels.Channels[102] );
								break;
							}

							var v = Externs.GetExternFloat( ((PushExternInputFloatData)op.data).extern_, ((PushExternInputFloatData)op.data).element );
							StackPush( v );
							break;

						case TfxBytecode.PushExternInputVec4:
							var PushExternInputVec4 = Externs.GetExternVec4( ((PushExternInputVec4Data)op.data).extern_, ((PushExternInputVec4Data)op.data).element );
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
							var index = ((PushGlobalChannelVectorData)op.data).Index;
							var global_channel = GlobalChannels?.Get( index ) ?? Vector4.Zero;
							//Log.Info( $"{Light}: {global_channel}" );
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

						case TfxBytecode.PopOutputMat4:
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
							Log.Error( $"{Name}: Not Implemented: {op.op}" );
							break;

					}
				}
			}
			catch ( Exception e )
			{
				Log.Error( $"{Name}: {e.Message}" );
			}
		} );
		return hlsl;
	}
}
