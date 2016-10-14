using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace FluidSym
{
    public class solver3d
    {
        public int dim;
        public float diff, visc;
        public float force, source;
        public int dvel;

        public Vector3[ , , ] vel;
        public Vector3[ , , ] vel_prev;
        public Vector4[ , , ] col;
        public Vector4[ , , ] col_prev;

        public solver3d(int N)
        {
            dim = N;
            vel = new Vector3[dim + 2, dim + 2, dim + 2];
            vel_prev = new Vector3[dim + 2, dim + 2, dim + 2];
            col = new Vector4[dim + 2, dim + 2, dim + 2];
            col_prev = new Vector4[dim + 2, dim + 2, dim + 2];
            diff = 0.0f;
            visc = 0.0f;
            force = 5.0f;
            source = 100.0f;
        }

        public void solve(float dt)
        {
            vel_step(dt);
            col_step(dt);
            for (int i = 0; i < dim + 2; i++)
            {
                for (int j = 0; j < dim + 2; j++)
                {
                    for (int k = 0; k < dim + 2; k++)
                    {
                        vel_prev[i, j, k] = Vector3.Zero;
                        col_prev[i, j, k] = Vector4.Zero;
                    }
                }
            }
        }

        public void add_dens_pos(int i, int j, int k, float s)
        {
            col_prev[i, j, k] = new Vector4(s, s, s, s);
        }

        public void add_vel_pos(int i, int j, int k, float force, Vector3 v)
        {
            vel_prev[i, j, k] = force * v;
        }

        void project()
        {
            float h = 1.0f / dim;
            //project
            float[, ,] div = new float[dim + 2, dim + 2, dim + 2];
            float[, ,] p = new float[dim + 2, dim + 2, dim + 2];
            for (int i = 1; i <= dim; i++)
            {
                for (int j = 1; j <= dim; j++)
                {
                    for (int k = 1; k <= dim; k++)
                    {
                        div[i, j, k] = -0.5f * h * (vel[i + 1, j, k].X - vel[i - 1, j, k].X +
                                                    vel[i, j + 1, k].Y - vel[i, j - 1, k].Y +
                                                    vel[i, j, k + 1].Z - vel[i, j, k - 1].Z);
                    }
                }
            }
            set_div_bnd(ref div);
            for (int i = 0; i < dim + 2; i++)
            {
                for (int j = 0; j < dim + 2; j++)
                {
                    for (int k = 0; k < dim + 2; k++)
                    {
                        p[i, j, k] = 0;
                    }
                }
            }
            //linear solve
            float a = 1;
            float c = 6;
            for (int step = 0; step < 20; step++)
            {
                for (int i = 1; i <= dim; i++)
                {
                    for (int j = 1; j <= dim; j++)
                    {
                        for (int k = 1; k <= dim; k++)
                        {
                            p[i, j, k] = (div[i, j, k] + a * (p[i - 1, j, k] + p[i + 1, j, k] +
                                                                     p[i, j - 1, k] + p[i, j + 1, k] +
                                                                     p[i, j, k - 1] + p[i, j, k + 1])) / c;

                        }
                    }
                }
                set_div_bnd(ref p);
            }

            for (int i = 1; i <= dim; i++)
            {
                for (int j = 1; j <= dim; j++)
                {
                    for (int k = 1; k <= dim; k++)
                    {
                        vel[i, j, k].X -= dim * (p[i + 1, j, k] - p[i - 1, j, k]) / 3;
                        vel[i, j, k].Y -= dim * (p[i, j + 1, k] - p[i, j - 1, k]) / 3;
                        vel[i, j, k].Z -= dim * (p[i, j, k + 1] - p[i, j, k - 1]) / 3;
                    }
                }
            }
            set_vel_bnd();
        }
        void set_vel_bnd()
        {
            for (int i = 1; i <= dim; i++)
            {
                for (int j = 1; j <= dim; j++)
                {
                    vel[0, i, j] = -vel[1, i, j];
                    vel[i, 0, j] = -vel[i, 1, j];
                    vel[i, j, 0] = -vel[i, j, 1];
                    vel[dim + 1, i, j] = -vel[dim, i, j];
                    vel[i, dim + 1, j] = -vel[i, dim, j];
                    vel[i, j, dim + 1] = -vel[i, j, dim];
                }
                vel[0, 0, i] = (vel[0, 1, i] + vel[1, 0, i]) * 0.5f;
                vel[0, i, 0] = (vel[0, i, 1] + vel[1, i, 0]) * 0.5f;
                vel[i, 0, 0] = (vel[i, 0, 1] + vel[i, 1, 0]) * 0.5f;
                vel[dim + 1, dim + 1, i] = (vel[dim + 1, dim, i] + vel[dim, dim + 1, i]) * 0.5f;
                vel[dim + 1, i, dim + 1] = (vel[dim + 1, i, dim] + vel[dim, i, dim + 1]) * 0.5f;
                vel[i, dim + 1, dim + 1] = (vel[i, dim + 1, dim] + vel[i, dim, dim + 1]) * 0.5f;
            }
            vel[0, 0, 0] = (vel[1, 0, 0] + vel[0, 1, 0] + vel[0, 0, 1]) / 3;
            vel[dim + 1, 0, 0] = (vel[dim + 1, 1, 0] + vel[dim + 1, 0, 1] + vel[dim, 0, 0]) / 3;
            vel[dim + 1, 0, dim + 1] = (vel[dim, 0, dim + 1] + vel[dim + 1, 0, dim] + vel[dim + 1, 1, dim + 1]) / 3;
            vel[0, 0, dim + 1] = (vel[1, 0, dim + 1] + vel[0, 1, dim + 1] + vel[0, 0, dim]) / 3;

            vel[dim + 1, dim + 1, dim + 1] = (vel[dim, dim + 1, dim + 1] + vel[dim + 1, dim, dim + 1] + vel[dim + 1, dim + 1, dim]) / 3;
            vel[0, dim + 1, dim + 1] = (vel[1, dim + 1, dim + 1] + vel[0, dim, dim + 1] + vel[0, dim + 1, dim]) / 3;
            vel[0, dim + 1, 0] = (vel[1, dim + 1, 0] + vel[0, dim, 0] + vel[0, dim + 1, 1]) / 3;
            vel[dim + 1, dim + 1, 0] = (vel[dim, dim + 1, 0] + vel[dim + 1, dim, 0] + vel[dim + 1, dim + 1, 1]) / 3;
        }
        public void vel_step(float dt)
        {
            Vector3[ , , ] swap;
            float h = 1.0f / dim;
            //add source
            for (int i = 0; i < dim + 2; i++)
            {
                for (int j = 0; j < dim + 2; j++)
                {
                    for (int k = 0; k < dim + 2; k++)
                    {
                        vel[i, j, k] += vel_prev[i, j, k] * dt;
                    }
                }
            }
            //swap
            swap = vel_prev;
            vel_prev = vel;
            vel = swap;
            //diffuse
            float a = dt * visc * dim * dim * dim;
            float c = 1 + 6 * a;
            for (int step = 0; step < 20; step++)
            {
                for (int i = 1; i <= dim; i++)
                {
                    for (int j = 1; j <= dim; j++)
                    {
                        for (int k = 1; k <= dim; k++)
                        {
                            vel[i, j, k] = (vel_prev[i, j, k] + a * (vel[i - 1, j, k] + vel[i + 1, j, k] +
                                                                     vel[i, j - 1, k] + vel[i, j + 1, k] +
                                                                     vel[i, j, k - 1] + vel[i, j, k + 1])) / c;

                        }
                    }
                }
                //set bound
                set_vel_bnd();
            }
           
            //project
            project();
            //swap
            swap = vel_prev;
            vel_prev = vel;
            vel = swap;

                        //advect
            int i0, i1, j0, j1, k0, k1;
            float x, y, z, s0, t0, u0, s1, t1, u1, dt0;
            dt0 = dt * dim;
            for (int i = 1; i <= dim; i++)
            {
                for (int j = 1; j <= dim; j++)
                {
                    for (int k = 1; k <= dim; k++)
                    {
                        x = i - dt0 * vel[i, j, k].X;
                        y = j - dt0 * vel[i, j, k].Y;
                        z = k - dt0 * vel[i, j, k].Z;

                        if (x < 0.5f)
                            x = 0.5f;
                        if (x > dim + 0.5f)
                            x = dim + 0.5f;
                        i0 = (int)x;
                        i1 = i0 + 1;

                        if (y < 0.5f)
                            y = 0.5f;
                        if (y > dim + 0.5f)
                            y = dim + 0.5f;
                        j0 = (int)y;
                        j1 = j0 + 1;

                        if (z < 0.5f)
                            z = 0.5f;
                        if (z > dim + 0.5f)
                            z = dim + 0.5f;
                        k0 = (int)z;
                        k1 = k0 + 1;

                        s1 = x - i0;
                        s0 = 1 - s1;
                        t1 = y - j0;
                        t0 = 1 - t1;
                        u1 = z - k0;
                        u0 = 1 - u1;

                        vel[i, j, k] = s0 * (t0 * (u0 * vel_prev[i0, j0, k0] + u1 * vel_prev[i0, j0, k1]) +
                                             t1 * (u0 * vel_prev[i0, j1, k0] + u1 * vel_prev[i0, j1, k1])) +
                                       s1 * (t0 * (u0 * vel_prev[i1, j0, k0] + u1 * vel_prev[i1, j0, k1]) +
                                             t1 * (u0 * vel_prev[i1, j1, k0] + u1 * vel_prev[i1, j1, k1]));
                    }
                }
            }
            project();

        }

        void set_div_bnd(ref float[ , , ] div)
        {
            for (int i = 1; i <= dim; i++)
            {
                for (int j = 1; j <= dim; j++)
                {
                    //div[0, i, j] = 0;
                    //div[i, 0, j] = 0;
                    //div[i, j, 0] = 0;
                    //div[dim + 1, i, j] = 0;
                    //div[i, dim + 1, j] = 0;
                    //div[i, j, dim + 1] = 0;
                    div[0, i, j] = div[1, i, j];
                    div[i, 0, j] = div[i, 1, j];
                    div[i, j, 0] = div[i, j, 1];
                    div[dim + 1, i, j] = div[dim, i, j];
                    div[i, dim + 1, j] = div[i, dim, j];
                    div[i, j, dim + 1] = div[i, j, dim];
                }
                div[0, 0, i] = (div[0, 1, i] + div[1, 0, i]) * 0.5f;
                div[0, i, 0] = (div[0, i, 1] + div[1, i, 0]) * 0.5f;
                div[i, 0, 0] = (div[i, 0, 1] + div[i, 1, 0]) * 0.5f;
                div[dim + 1, dim + 1, i] = (div[dim + 1, dim, i] + div[dim, dim + 1, i]) * 0.5f;
                div[dim + 1, i, dim + 1] = (div[dim + 1, i, dim] + div[dim, i, dim + 1]) * 0.5f;
                div[i, dim + 1, dim + 1] = (div[i, dim + 1, dim] + div[i, dim, dim + 1]) * 0.5f;
            }
            div[0, 0, 0] = (div[1, 0, 0] + div[0, 1, 0] + div[0, 0, 1]) / 3;
            div[dim + 1, 0, 0] = (div[dim + 1, 1, 0] + div[dim + 1, 0, 1] + div[dim, 0, 0]) / 3;
            div[dim + 1, 0, dim + 1] = (div[dim, 0, dim + 1] + div[dim + 1, 0, dim] + div[dim + 1, 1, dim + 1]) / 3;
            div[0, 0, dim + 1] = (div[1, 0, dim + 1] + div[0, 1, dim + 1] + div[0, 0, dim]) / 3;

            div[dim + 1, dim + 1, dim + 1] = (div[dim, dim + 1, dim + 1] + div[dim + 1, dim, dim + 1] + div[dim + 1, dim + 1, dim]) / 3;
            div[0, dim + 1, dim + 1] = (div[1, dim + 1, dim + 1] + div[0, dim, dim + 1] + div[0, dim + 1, dim]) / 3;
            div[0, dim + 1, 0] = (div[1, dim + 1, 0] + div[0, dim, 0] + div[0, dim + 1, 1]) / 3;
            div[dim + 1, dim + 1, 0] = (div[dim, dim + 1, 0] + div[dim + 1, dim, 0] + div[dim + 1, dim + 1, 1]) / 3;
        }
        void set_col_bnd()
        {
            for (int i = 1; i <= dim; i++)
            {
                for (int j = 1; j <= dim; j++)
                {
                    col[0, i, j] = col[1, i, j];
                    col[i, 0, j] = col[i, 1, j];
                    col[i, j, 0] = col[i, j, 1];
                    col[dim + 1, i, j] = col[dim, i, j];
                    col[i, dim + 1, j] = col[i, dim, j];
                    col[i, j, dim + 1] = col[i, j, dim];
                    //col[0, i, j] = Vector4.Zero;
                    //col[i, 0, j] = Vector4.Zero;
                    //col[i, j, 0] = Vector4.Zero;
                    //col[dim + 1, i, j] = Vector4.Zero;
                    //col[i, dim + 1, j] = Vector4.Zero;
                    //col[i, j, dim + 1] = Vector4.Zero;
                }
                col[0, 0, i] = (col[0, 1, i] + col[1, 0, i]) * 0.5f;
                col[0, i, 0] = (col[0, i, 1] + col[1, i, 0]) * 0.5f;
                col[i, 0, 0] = (col[i, 0, 1] + col[i, 1, 0]) * 0.5f;
                col[dim + 1, dim + 1, i] = (col[dim + 1, dim, i] + col[dim, dim + 1, i]) * 0.5f;
                col[dim + 1, i, dim + 1] = (col[dim + 1, i, dim] + col[dim, i, dim + 1]) * 0.5f;
                col[i, dim + 1, dim + 1] = (col[i, dim + 1, dim] + col[i, dim, dim + 1]) * 0.5f;  
            }
            col[0, 0, 0] = (col[1, 0, 0] + col[0, 1, 0] + col[0, 0, 1]) / 3;
            col[dim + 1, 0, 0] = (col[dim + 1, 1, 0] + col[dim + 1, 0, 1] + col[dim, 0, 0]) / 3;
            col[dim + 1, 0, dim + 1] = (col[dim, 0, dim + 1] + col[dim + 1, 0, dim] + col[dim + 1, 1, dim + 1]) / 3;
            col[0, 0, dim + 1] = (col[1, 0, dim + 1] + col[0, 1, dim + 1] + col[0, 0, dim]) / 3;

            col[dim + 1, dim + 1, dim + 1] = (col[dim, dim + 1, dim + 1] + col[dim + 1, dim, dim + 1] + col[dim + 1, dim + 1, dim]) / 3;
            col[0, dim + 1, dim + 1] = (col[1, dim + 1, dim + 1] + col[0, dim, dim + 1] + col[0, dim + 1, dim]) / 3;
            col[0, dim + 1, 0] = (col[1, dim + 1, 0] + col[0, dim, 0] + col[0, dim + 1, 1]) / 3;
            col[dim + 1, dim + 1, 0] = (col[dim, dim + 1, 0] + col[dim + 1, dim, 0] + col[dim + 1, dim + 1, 1]) / 3;
        }
        public void col_step(float dt)
        {
            Vector4[ , , ] swap;
            //add source
            for (int i = 0; i < dim + 2; i++)
            {
                for (int j = 0; j < dim + 2; j++)
                {
                    for (int k = 0; k < dim + 2; k++)
                    {
                        col[i, j, k] += col_prev[i, j, k] * dt;
                    }
                }
            }
            //swap
            swap = col_prev;
            col_prev = col;
            col = swap;
            //diffuse
            float a = dt * diff * dim * dim * dim;
            float c = 1 + 6 * a;
            for (int step = 0; step < 20; step++)
            {
                for (int i = 1; i <= dim; i++)
                {
                    for (int j = 1; j <= dim; j++)
                    {
                        for (int k = 1; k <= dim; k++)
                        {
                            col[i, j, k] = (col_prev[i, j, k] + a * (col[i - 1, j, k] + col[i + 1, j, k] +
                                                                     col[i, j - 1, k] + col[i, j + 1, k] +
                                                                     col[i, j, k - 1] + col[i, j, k + 1])) / c;

                        }
                    }
                }
                //set bound
                set_col_bnd();
                
            }
            //swap
            swap = col_prev;
            col_prev = col;
            col = swap;
            //advect
            int i0, i1, j0, j1, k0, k1;
            float x, y, z, s0, t0, u0, s1, t1, u1, dt0;
            dt0 = dt * dim;
            for (int i = 1; i <= dim; i++)
            {
                for (int j = 1; j <= dim; j++)
                {
                    for (int k = 1; k <= dim; k++)
                    {
                        x = i - dt0 * vel[i, j, k].X;
                        y = j - dt0 * vel[i, j, k].Y;
                        z = k - dt0 * vel[i, j, k].Z;

                        if (x < 0.5f)
                            x = 0.5f;
                        if (x > dim + 0.5f)
                            x = dim + 0.5f;
                        i0 = (int)x;
                        i1 = i0 + 1;

                        if (y < 0.5f)
                            y = 0.5f;
                        if (y > dim + 0.5f)
                            y = dim + 0.5f;
                        j0 = (int)y;
                        j1 = j0 + 1;

                        if (z < 0.5f)
                            z = 0.5f;
                        if (z > dim + 0.5f)
                            z = dim + 0.5f;
                        k0 = (int)z;
                        k1 = k0 + 1;

                        s1 = x - i0;
                        s0 = 1 - s1;
                        t1 = y - j0;
                        t0 = 1 - t1;
                        u1 = z - k0;
                        u0 = 1 - u1;

                        col[i, j, k] = s0 * (t0 * (u0 * col_prev[i0, j0, k0] + u1 * col_prev[i0, j0, k1]) +
                                             t1 * (u0 * col_prev[i0, j1, k0] + u1 * col_prev[i0, j1, k1])) +
                                       s1 * (t0 * (u0 * col_prev[i1, j0, k0] + u1 * col_prev[i1, j0, k1]) +
                                             t1 * (u0 * col_prev[i1, j1, k0] + u1 * col_prev[i1, j1, k1]));
                    }
                }
            }
            //set bound
            set_col_bnd();
        }

    }
}

