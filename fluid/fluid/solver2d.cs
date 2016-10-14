using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace FluidSym
{
    public class solver2d
    {
        public int dim;
        public float diff, visc;
        public float force, source;
        public int dvel;

        public Vector2[,] vel;
        public Vector2[,] vel_prev;
        public float[, ] col;
        public float[, ] col_prev;

        public solver2d(int N)
        {
            dim = N;
            vel = new Vector2[dim + 2, dim + 2];
            vel_prev = new Vector2[dim + 2, dim + 2];
            col = new float[dim + 2, dim + 2];
            col_prev = new float[dim + 2, dim + 2];
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
                    vel_prev[i, j] = Vector2.Zero;
                    col_prev[i, j] = 0;
                }
            }
        }

        public void add_dens_pos(int i, int j, int k, float s)
        {
            col_prev[i, j] = s;
        }

        public void add_vel_pos(int i, int j, int k, float force, Vector2 v)
        {
            vel_prev[i, j] = force * v;
        }

        void project()
        {
            float h = 1.0f / dim;
            //project
            float[, ] div = new float[dim + 2, dim + 2];
            float[, ] p = new float[dim + 2, dim + 2];
            for (int i = 1; i <= dim; i++)
            {
                for (int j = 1; j <= dim; j++)
                {
                        div[i, j] = -0.5f * h * (vel[i + 1, j].X - vel[i - 1, j].X +
                                                    vel[i, j + 1].Y - vel[i, j - 1].Y);
                }
            }
            set_div_bnd(ref div);
            for (int i = 0; i < dim + 2; i++)
            {
                for (int j = 0; j < dim + 2; j++)
                {
                        p[i, j] = 0;
                }
            }
            //linear solve
            float a = 1;
            float c = 4;
            for (int step = 0; step < 20; step++)
            {
                for (int i = 1; i <= dim; i++)
                {
                    for (int j = 1; j <= dim; j++)
                    {
                            p[i, j] = (div[i, j] + a * (p[i - 1, j] + p[i + 1, j] +
                                                        p[i, j - 1] + p[i, j + 1])) / c;

                    }
                }
                set_div_bnd(ref p);
            }

            for (int i = 1; i <= dim; i++)
            {
                for (int j = 1; j <= dim; j++)
                {
                        vel[i, j].X -= dim * (p[i + 1, j] - p[i - 1, j]) / 2;
                        vel[i, j].Y -= dim * (p[i, j + 1] - p[i, j - 1]) / 2;
                }
            }
            set_vel_bnd();
        }
        void set_vel_bnd()
        {
            for (int i = 1; i <= dim; i++)
            {
                vel[0, i] = -vel[1, i];
                vel[i, 0] = -vel[i, 1];
                vel[dim + 1, i] = -vel[dim, i];
                vel[i, dim + 1] = -vel[i, dim];
            }
                vel[0, 0] = (vel[0, 1] + vel[1, 0]) * 0.5f;
                vel[0, dim + 1] = (vel[1, dim + 1] + vel[0, dim]) * 0.5f;
                vel[dim + 1, 0] = (vel[dim, 0] + vel[dim + 1, 1]) * 0.5f;
                vel[dim + 1, dim + 1] = (vel[dim + 1, dim] + vel[dim, dim + 1]) * 0.5f;
        }
        public void vel_step(float dt)
        {
            Vector2[,] swap;
            float h = 1.0f / dim;
            //add source
            for (int i = 0; i < dim + 2; i++)
            {
                for (int j = 0; j < dim + 2; j++)
                {
                        vel[i, j] += vel_prev[i, j] * dt;
                }
            }
            //swap
            swap = vel_prev;
            vel_prev = vel;
            vel = swap;
            //diffuse
            float a = dt * visc * dim * dim;
            float c = 1 + 4 * a;
            for (int step = 0; step < 20; step++)
            {
                for (int i = 1; i <= dim; i++)
                {
                    for (int j = 1; j <= dim; j++)
                    {

                            vel[i, j] = (vel_prev[i, j] + a * (vel[i - 1, j] + vel[i + 1, j] +
                                                               vel[i, j - 1] + vel[i, j + 1])) / c;

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
            int i0, i1, j0, j1;
            float x, y, z, s0, t0, s1, t1, dt0;
            dt0 = dt * dim;
            for (int i = 1; i <= dim; i++)
            {
                for (int j = 1; j <= dim; j++)
                {
                    x = i - dt0 * vel[i, j].X;
                    y = j - dt0 * vel[i, j].Y;

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

                    s1 = x - i0;
                    s0 = 1 - s1;
                    t1 = y - j0;
                    t0 = 1 - t1;

                    vel[i, j] = s0 * (t0 * vel_prev[i0, j0] + t1 * vel_prev[i0, j1]) +
                                s1 * (t0 * vel_prev[i1, j0] + t1 * vel_prev[i1, j1]);
                }
            }
            project();
        }

        void set_div_bnd(ref float[,] div)
        {
            for (int i = 1; i <= dim; i++)
            {
                div[0, i] = div[1, i];
                div[i, 0] = div[i, 1];
                div[dim + 1, i] = div[dim, i];
                div[i, dim + 1] = div[i, dim];
            }
            div[0, 0] = (div[0, 1] + div[1, 0]) * 0.5f;
            div[0, dim + 1] = (div[1, dim + 1] + div[0, dim]) * 0.5f;
            div[dim + 1, 0] = (div[dim, 0] + div[dim + 1, 1]) * 0.5f;
            div[dim + 1, dim + 1] = (div[dim + 1, dim] + div[dim, dim + 1]) * 0.5f;
        }
        void set_col_bnd()
        {
            for (int i = 1; i <= dim; i++)
            {
                col[0, i] = col[1, i];
                col[i, 0] = col[i, 1];
                col[dim + 1, i] = col[dim, i];
                col[i, dim + 1] = col[i, dim];
            }
            col[0, 0] = (col[0, 1] + col[1, 0]) * 0.5f;
            col[0, dim + 1] = (col[1, dim + 1] + col[0, dim]) * 0.5f;
            col[dim + 1, 0] = (col[dim, 0] + col[dim + 1, 1]) * 0.5f;
            col[dim + 1, dim + 1] = (col[dim + 1, dim] + col[dim, dim + 1]) * 0.5f;
        }
        public void col_step(float dt)
        {
            float[,] swap;
            //add source
            for (int i = 0; i < dim + 2; i++)
            {
                for (int j = 0; j < dim + 2; j++)
                {
                        col[i, j] += col_prev[i, j] * dt;
                }
            }
            //swap
            swap = col_prev;
            col_prev = col;
            col = swap;
            //diffuse
            float a = dt * diff * dim * dim;
            float c = 1 + 4 * a;
            for (int step = 0; step < 20; step++)
            {
                for (int i = 1; i <= dim; i++)
                {
                    for (int j = 1; j <= dim; j++)
                    {
                            col[i, j] = (col_prev[i, j] + a * (col[i - 1, j] + col[i + 1, j] +
                                                                     col[i, j - 1] + col[i, j + 1])) / c;

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
            int i0, i1, j0, j1;
            float x, y, z, s0, t0, s1, t1, dt0;
            dt0 = dt * dim;
            for (int i = 1; i <= dim; i++)
            {
                for (int j = 1; j <= dim; j++)
                {
                    x = i - dt0 * vel[i, j].X;
                    y = j - dt0 * vel[i, j].Y;

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

                    s1 = x - i0;
                    s0 = 1 - s1;
                    t1 = y - j0;
                    t0 = 1 - t1;

                    col[i, j] = s0 * (t0 * col_prev[i0, j0] + t1 * col_prev[i0, j1]) +
                                s1 * (t0 * col_prev[i1, j0] + t1 * col_prev[i1, j1]);
                }
            }
            //set bound
            set_col_bnd();
        }

    }
}

