using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

namespace MegaFiers
{
	[AddComponentMenu("Modifiers/Warps/Bend")]
	public class MegaBendWarp : MegaWarp
	{
		public bool		useRadius	= false;
		public float	radius		= 10.0f;
		public float	angle		= 0.0f;
		public float	dir			= 0.0f;
		public MegaAxis	axis		= MegaAxis.X;
		public bool		doRegion	= false;
		public float	from		= 0.0f;
		public float	to			= 0.0f;
		Matrix4x4		mat			= new Matrix4x4();
		Matrix4x4		tmAbove		= new Matrix4x4();
		Matrix4x4		tmBelow		= new Matrix4x4();
		float			r			= 0.0f;
		Job				job;
		JobHandle		jobHandle;

		public override string WarpName() { return "Bend"; }
		public override string GetIcon() { return "MegaBend icon.png"; }
		public override string GetHelpURL() { return "?page_id=2551"; }

		[BurstCompile]
		struct Job : IJobParallelFor
		{
			public bool					doRegion;
			public float				from;
			public float				to;
			public Matrix4x4			tmBelow;
			public Matrix4x4			tmAbove;
			public float				totaldecay;
			public float				r;
			public NativeArray<Vector3> jvertices;
			public NativeArray<Vector3> jsverts;
			public Matrix4x4			tm;
			public Matrix4x4			invtm;
			public Matrix4x4			wtm;
			public Matrix4x4			winvtm;

			public void Execute(int vi)
			{
				float3 p = wtm.MultiplyPoint3x4(jvertices[vi]);

				if ( r == 0.0f && !doRegion )
				{
					jsverts[vi] = winvtm.MultiplyPoint3x4(p);   //jvertices[vi];
					return;
				}

				p = tm.MultiplyPoint3x4(p);

				if ( doRegion )
				{
					if ( p.y <= from )
					{
						jsverts[vi] = invtm.MultiplyPoint3x4(tmBelow.MultiplyPoint3x4(p));
						return;
					}
					else
					{
						if ( p.y >= to )
						{
							jsverts[vi] = invtm.MultiplyPoint3x4(tmAbove.MultiplyPoint3x4(p));
							return;
						}
					}
				}

				if ( r == 0.0f )
				{
					jsverts[vi] = jvertices[vi];
					return;
				}

				float3 ip = p;
				float dist = math.length(p);
				float dcy = math.exp(-totaldecay * dist);

				float x = p.x;
				float y = p.y;

				float yr = y / r;

				float c = math.cos(math.PI - yr);
				float s = math.sin(math.PI - yr);
				float px = r * c + r - x * c;
				p.x = px;
				float pz = r * s - x * s;
				p.y = pz;

				p = Vector3.Lerp(ip, p, dcy);

				p = invtm.MultiplyPoint3x4(p);

				jsverts[vi] = winvtm.MultiplyPoint3x4(p);
			}
		}

		public override void Modify(MegaWarpBind mod)	//MegaModifiers mc)
		{
			if ( mod.verts != null )
			{
				job.doRegion	= doRegion;
				job.from		= from;
				job.to			= to;
				job.tmBelow		= tmBelow;
				job.tmAbove		= tmAbove;
				job.r			= r;
				job.totaldecay	= totaldecay;
				job.tm			= tm;
				job.invtm		= invtm;
				job.wtm			= mod.tm;
				job.winvtm		= mod.invtm;
				job.jvertices	= mod.jverts;
				job.jsverts		= mod.jsverts;

				jobHandle = job.Schedule(mod.jverts.Length, 64);
				jobHandle.Complete();
			}
		}

		void CalcR(MegaAxis axis, float ang)
		{
			if ( useRadius )
			{
				r = radius;
			}
			else
			{
				float len = 0.0f;

				if ( !doRegion )
				{
					switch ( axis )
					{
						case MegaAxis.X: len = Width; break;
						case MegaAxis.Z: len = Height; break;
						case MegaAxis.Y: len = Length; break;
					}
				}
				else
					len = to - from;

				if ( Mathf.Abs(ang) < 0.000001f )
					r = 0.0f;
				else
					r = len / ang;
			}
		}

		public override Vector3 Map(int i, Vector3 p)
		{
			if ( r == 0.0f && !doRegion )
				return p;

			p = tm.MultiplyPoint3x4(p);

			if ( doRegion )
			{
				if ( p.y <= from )
					return invtm.MultiplyPoint3x4(tmBelow.MultiplyPoint3x4(p));
				else
				{
					if ( p.y >= to )
						return invtm.MultiplyPoint3x4(tmAbove.MultiplyPoint3x4(p));
				}
			}

			if ( r == 0.0f )
				return invtm.MultiplyPoint3x4(p);

			Vector3 ip = p;
			float dist = p.magnitude;
			float dcy = Mathf.Exp(-totaldecay * Mathf.Abs(dist));

			float x = p.x;
			float y = p.y;

			float yr = y / r;

			float c = Mathf.Cos(Mathf.PI - yr);
			float s = Mathf.Sin(Mathf.PI - yr);
			float px = r * c + r - x * c;
			p.x = px;
			float pz = r * s - x * s;
			p.y = pz;

			p = Vector3.Lerp(ip, p, dcy);

			p = invtm.MultiplyPoint3x4(p);
			return p;
		}

		void Calc()
		{
			tm = transform.worldToLocalMatrix;
			invtm = tm.inverse;

			mat = Matrix4x4.identity;

			switch ( axis )
			{
				case MegaAxis.X: MegaMatrix.RotateZ(ref mat, Mathf.PI * 0.5f); break;
				case MegaAxis.Y: MegaMatrix.RotateX(ref mat, -Mathf.PI * 0.5f); break;
				case MegaAxis.Z: break;
			}

			MegaMatrix.RotateY(ref mat, Mathf.Deg2Rad * dir);
			SetAxis(mat);

			CalcR(axis, Mathf.Deg2Rad * -angle);

			if ( doRegion )
			{
				doRegion = false;
				float len  = to - from;
				float rat1, rat2;

				if ( len == 0.0f )
					rat1 = rat2 = 1.0f;
				else
				{
					rat1 = to / len;
					rat2 = from / len;
				}

				Vector3 pt;
				tmAbove = Matrix4x4.identity;
				MegaMatrix.Translate(ref tmAbove, 0.0f, -to, 0.0f);
				MegaMatrix.RotateZ(ref tmAbove, -Mathf.Deg2Rad * angle * rat1);
				MegaMatrix.Translate(ref tmAbove, 0.0f, to, 0.0f);
				pt = new Vector3(0.0f, to, 0.0f);
				MegaMatrix.Translate(ref tmAbove, tm.MultiplyPoint3x4(Map(0, invtm.MultiplyPoint3x4(pt))) - pt);

				tmBelow = Matrix4x4.identity;
				MegaMatrix.Translate(ref tmBelow, 0.0f, -from, 0.0f);
				MegaMatrix.RotateZ(ref tmBelow, -Mathf.Deg2Rad * angle * rat2);
				MegaMatrix.Translate(ref tmBelow, 0.0f, from, 0.0f);
				pt = new Vector3(0.0f, from, 0.0f);
				MegaMatrix.Translate(ref tmBelow, tm.MultiplyPoint3x4(Map(0, invtm.MultiplyPoint3x4(pt))) - pt);

				doRegion = true;
			}
		}

		public override bool Prepare(float decay)
		{
			Calc();

			totaldecay = Decay + decay;
			if ( totaldecay < 0.0f )
				totaldecay = 0.0f;

			return true;
		}

		public override void ExtraGizmo()
		{
			if ( doRegion )
				DrawFromTo(axis, from, to);
		}
	}
}