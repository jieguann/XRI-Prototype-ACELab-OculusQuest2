using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

namespace MegaFiers
{
	[AddComponentMenu("Modifiers/Warps/Melt")]
	public class MegaMeltWarp : MegaWarp
	{
		public override string WarpName() { return "Melt"; }
		public override string GetIcon() { return "MegaMelt icon.png"; }
		public override string GetHelpURL() { return "?page_id=2566"; }

		public float		Amount			= 0.0f;
		public float		Spread			= 19.0f;
		public MegaMeltMat	MaterialType	= MegaMeltMat.Ice;
		public float		Solidity		= 1.0f;
		public MegaAxis		axis			= MegaAxis.X;
		public bool			FlipAxis		= false;
		float				zba				= 0.0f;
		float				size			= 0.0f;
		float				bulger			= 0.0f;
		float				ybr,zbr,visvaluea;
		int					confiner,vistypea;
		float				cx,cy,cz;
		float				xsize,ysize,zsize;
		float				ooxsize,ooysize,oozsize;
		public float		flatness = 0.1f;
		Job					job;
		JobHandle			jobHandle;

		float hypot(float x, float y)
		{
			return Mathf.Sqrt(x * x + y * y);
		}

		[BurstCompile]
		struct Job : IJobParallelFor
		{
			public float				bulger;
			public MegaAxis				axis;
			public float				size;
			public float3				min;
			public float3				max;
			public float				cx;
			public float				cy;
			public float				cz;
			public bool					FlipAxis;
			public float				zbr;
			public float				visvaluea;
			public float				flatness;
			public float				ooxsize;
			public float				ooysize;
			public float				oozsize;
			public float				ybr;
			public float				totaldecay;
			public NativeArray<Vector3> jvertices;
			public NativeArray<Vector3> jsverts;
			public Matrix4x4			tm;
			public Matrix4x4			invtm;
			public Matrix4x4			wtm;
			public Matrix4x4			winvtm;

			float hypot(float x, float y)
			{
				return math.sqrt(x * x + y * y);
			}

			public void Execute(int vi)
			{
				float3 p = wtm.MultiplyPoint3x4(jvertices[vi]);

				float x, y, z;
				float xw, yw, zw;
				float vdist, mfac, dx, dy;
				float defsinex = 0.0f, coldef = 0.0f, realmax = 0.0f;

				p = tm.MultiplyPoint3x4(p);

				Vector3 ip = p;

				x = p.x; y = p.y; z = p.z;
				xw = x - cx; yw = y - cy; zw = z - cz;

				if ( xw == 0.0f && yw == 0.0f && zw == 0.0f ) xw = yw = zw = 1.0f;
				if ( x == 0.0f && y == 0.0f && z == 0.0f ) x = y = z = 1.0f;

				// Find distance from centre
				vdist = math.sqrt(xw * xw + yw * yw + zw * zw);

				float dcy = math.exp(-totaldecay * vdist);

				mfac = size / vdist;

				if ( axis == MegaAxis.Z )
				{
					dx = xw + math.sign(xw) * ((math.abs(xw * mfac)) * (bulger * ybr));
					dy = yw + math.sign(yw) * ((math.abs(yw * mfac)) * (bulger * ybr));
					x = (dx + cx);
					y = (dy + cy);
				}

				if ( axis == MegaAxis.Y )
				{
					dx = xw + math.sign(xw) * ((math.abs(xw * mfac)) * (bulger * ybr));
					dy = zw + math.sign(zw) * ((math.abs(zw * mfac)) * (bulger * ybr));
					x = (dx + cx);
					z = (dy + cz);
				}

				if ( axis == MegaAxis.X )
				{
					dx = zw + math.sign(zw) * ((math.abs(zw * mfac)) * (bulger * ybr));
					dy = yw + math.sign(yw) * ((math.abs(yw * mfac)) * (bulger * ybr));
					z = (dx + cz);
					y = (dy + cy);
				}

				if ( axis == MegaAxis.Z ) realmax = hypot((max.x - cx), (max.y - cy));
				if ( axis == MegaAxis.Y ) realmax = hypot((max.x - cx), (max.z - cz));
				if ( axis == MegaAxis.X ) realmax = hypot((max.z - cz), (max.y - cy));

				if ( axis == MegaAxis.Z )
				{
					defsinex = hypot((x - cx), (y - cy));
					coldef = realmax - hypot((x - cx), (y - cy));
				}

				if ( axis == MegaAxis.Y )
				{
					defsinex = hypot((x - cx), (z - cz));
					coldef = realmax - hypot((x - cx), (z - cz));
				}

				if ( axis == MegaAxis.X )
				{
					defsinex = hypot((z - cz), (y - cy));
					coldef = realmax - hypot((z - cz), (y - cy));
				}

				if ( coldef < 0.0f )
					coldef = 0.0f;

				defsinex += (coldef / visvaluea);

				if ( axis == MegaAxis.Z )
				{
					if ( FlipAxis )
					{
						float nminz = min.z + (((z - min.z) * oozsize) * flatness);
						z -= (defsinex * bulger);
						if ( z <= nminz ) z = nminz;
						if ( z <= (nminz + zbr) ) z = (nminz + zbr);
					}
					else
					{
						float nmaxz = max.z - (((max.z - z) * oozsize) * flatness);
						z += (defsinex * bulger);
						if ( z >= nmaxz ) z = nmaxz;
						if ( z >= (nmaxz + zbr) ) z = (nmaxz + zbr);
					}
				}
				if ( axis == MegaAxis.Y )
				{
					if ( !FlipAxis )
					{
						float nminy = min.y + (((y - min.y) * ooysize) * flatness);

						y -= (defsinex * bulger);

						if ( y <= nminy ) y = nminy;
						if ( y <= (nminy + zbr) ) y = (nminy + zbr);
					}
					else
					{
						float nmaxy = max.y - (((max.y - y) * ooysize) * flatness);
						y += (defsinex * bulger);
						if ( y >= nmaxy ) y = nmaxy;
						if ( y >= (nmaxy + zbr) ) y = (nmaxy + zbr);
					}
				}
				if ( axis == MegaAxis.X )
				{
					if ( !FlipAxis )
					{
						float nminx = min.x + (((x - min.x) * ooxsize) * flatness);
						x -= (defsinex * bulger);
						if ( x <= nminx ) x = nminx;
						if ( x <= (nminx + zbr) ) x = (nminx + zbr);
					}
					else
					{
						float nmaxx = max.x - (((max.x - x) * ooxsize) * flatness);
						x += (defsinex * bulger);
						if ( x >= nmaxx ) x = nmaxx;
						if ( x >= (nmaxx + zbr) ) x = (nmaxx + zbr);
					}
				}

				p.x = x; p.y = y; p.z = z;

				p = Vector3.Lerp(ip, p, dcy);

				p = invtm.MultiplyPoint3x4(p);

				jsverts[vi] = winvtm.MultiplyPoint3x4(p);
			}
		}

		public override void Modify(MegaWarpBind mod)
		{
			if ( mod.verts != null )
			{
				job.bulger		= bulger;
				job.axis		= axis;
				job.size		= size;
				job.min			= min;
				job.max			= max;
				job.cx			= cx;
				job.cy			= cy;
				job.cz			= cz;
				job.FlipAxis	= FlipAxis;
				job.zbr			= zbr;
				job.visvaluea	= visvaluea;
				job.flatness	= flatness;
				job.ooxsize		= ooxsize;
				job.ooysize		= ooysize;
				job.oozsize		= oozsize;
				job.ybr			= ybr;
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

		public override Vector3 Map(int i, Vector3 p)
		{
			float x, y, z;
			float xw,yw,zw;
			float vdist,mfac,dx,dy;
			float defsinex = 0.0f, coldef = 0.0f, realmax = 0.0f;

			p = tm.MultiplyPoint3x4(p);

			Vector3 ip = p;

			x = p.x; y = p.y; z = p.z;
			xw = x - cx; yw = y - cy; zw = z - cz;

			if ( xw == 0.0f && yw == 0.0f && zw == 0.0f ) xw = yw = zw = 1.0f;
			if ( x == 0.0f && y == 0.0f && z == 0.0f ) x = y = z = 1.0f;

			// Find distance from centre
			vdist = Mathf.Sqrt(xw * xw + yw * yw + zw * zw);

			float dcy = Mathf.Exp(-totaldecay * Mathf.Abs(vdist));

			mfac = size / vdist;

			if ( axis == MegaAxis.Z )
			{
				dx = xw + Mathf.Sign(xw) * ((Mathf.Abs(xw * mfac)) * (bulger * ybr));
				dy = yw + Mathf.Sign(yw) * ((Mathf.Abs(yw * mfac)) * (bulger * ybr));
				x = (dx + cx);
				y = (dy + cy);
			}

			if ( axis == MegaAxis.Y )
			{
				dx = xw + Mathf.Sign(xw) * ((Mathf.Abs(xw * mfac)) * (bulger * ybr));
				dy = zw + Mathf.Sign(zw) * ((Mathf.Abs(zw * mfac)) * (bulger * ybr));
				x = (dx + cx);
				z = (dy + cz);
			}

			if ( axis == MegaAxis.X )
			{
				dx = zw + Mathf.Sign(zw) * ((Mathf.Abs(zw * mfac)) * (bulger * ybr));
				dy = yw + Mathf.Sign(yw) * ((Mathf.Abs(yw * mfac)) * (bulger * ybr));
				z = (dx + cz);
				y = (dy + cy);
			}

			if ( axis == MegaAxis.Z ) realmax = hypot((max.x - cx), (max.y - cy));
			if ( axis == MegaAxis.Y ) realmax = hypot((max.x - cx), (max.z - cz));
			if ( axis == MegaAxis.X ) realmax = hypot((max.z - cz), (max.y - cy));

			if ( axis == MegaAxis.Z )
			{
				defsinex = hypot((x - cx), (y - cy));
				coldef = realmax - hypot((x - cx), (y - cy));
			}

			if ( axis == MegaAxis.Y )
			{
				defsinex = hypot((x - cx), (z - cz));
				coldef = realmax - hypot((x - cx), (z - cz));
			}

			if ( axis == MegaAxis.X )
			{
				defsinex = hypot((z - cz), (y - cy));
				coldef = realmax - hypot((z - cz), (y - cy));
			}

			if ( coldef < 0.0f )
				coldef = 0.0f;

			defsinex += (coldef / visvaluea);

			if ( axis == MegaAxis.Z )
			{
				if ( FlipAxis )
				{
					float nminz = min.z + (((z - min.z) * oozsize) * flatness);
					z -= (defsinex * bulger);
					if ( z <= nminz ) z = nminz;
					if ( z <= (nminz + zbr) ) z = (nminz + zbr);
				}
				else
				{
					float nmaxz = max.z - (((max.z - z) * oozsize) * flatness);
					z += (defsinex * bulger);
					if ( z >= nmaxz ) z = nmaxz;
					if ( z >= (nmaxz + zbr) ) z = (nmaxz + zbr);
				}
			}
			if ( axis == MegaAxis.Y )
			{
				if ( !FlipAxis )
				{
					float nminy = min.y + (((y - min.y) * ooysize) * flatness);

					y -= (defsinex * bulger);

					if ( y <= nminy ) y = nminy;
					if ( y <= (nminy + zbr) ) y = (nminy + zbr);
				}
				else
				{
					float nmaxy = max.y - (((max.y - y) * ooysize) * flatness);
					y += (defsinex * bulger);
					if ( y >= nmaxy ) y = nmaxy;
					if ( y >= (nmaxy + zbr) ) y = (nmaxy + zbr);
				}
			}
			if ( axis == MegaAxis.X )
			{
				if ( !FlipAxis )
				{
					float nminx = min.x + (((x - min.x) * ooxsize) * flatness);
					x -= (defsinex * bulger);
					if ( x <= nminx ) x = nminx;
					if ( x <= (nminx + zbr) ) x = (nminx + zbr);
				}
				else
				{
					float nmaxx = max.x - (((max.x - x) * ooxsize) * flatness);
					x += (defsinex * bulger);
					if ( x >= nmaxx ) x = nmaxx;
					if ( x >= (nmaxx + zbr) ) x = (nmaxx + zbr);
				}
			}

			p.x = x; p.y = y; p.z = z;

			p = Vector3.Lerp(ip, p, dcy);

			p = invtm.MultiplyPoint3x4(p);
			return p;
		}

		Vector3 max = Vector3.zero;
		Vector3 min = Vector3.zero;

		public override bool Prepare(float decay)
		{
			tm = transform.worldToLocalMatrix;
			invtm = tm.inverse;

			cx = 0.0f;
			cz = 0.0f;
			cy = 0.0f;

			// Compute the size and center
			xsize = Width;
			ysize = Height;
			zsize = Length;

			ooxsize = 1.0f / xsize;
			ooysize = 1.0f / ysize;
			oozsize = 1.0f / zsize;

			size = (xsize > ysize) ? xsize : ysize;
			size = (zsize > size) ? zsize : size;
			size /= 2.0f;

			min.x = -Width * 0.5f;
			min.z = -Length * 0.5f;
			min.y = 0.0f;

			max.x = Width * 0.5f;
			max.z = Length * 0.5f;
			max.y = Height;

			switch ( MaterialType )
			{
				case MegaMeltMat.Ice: visvaluea = 2.0f; break;
				case MegaMeltMat.Glass: visvaluea = 12.0f; break;
				case MegaMeltMat.Jelly: visvaluea = 0.4f; break;
				case MegaMeltMat.Plastic: visvaluea = 0.7f; break;
				case MegaMeltMat.Custom: visvaluea = Solidity; break;
			}

			if ( Amount < 0.0f )
				Amount = 0.0f;

			ybr = Spread / 100.0f;
			zbr = zba / 10.0f;
			bulger = Amount / 100.0f;

			totaldecay = Decay + decay;
			if ( totaldecay < 0.0f )
				totaldecay = 0.0f;

			return true;
		}
	}
}