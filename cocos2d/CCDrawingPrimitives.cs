using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace cocos2d
{
    public class CCDrawingPrimitives
    {
        private static PrimitiveBatch m_Batch;
        private static float m_PointSize = 3f;
        private static ccColor4B m_Color;

        public static void Init(GraphicsDevice graphics)
        {
            m_Batch = new PrimitiveBatch(graphics);
        }

        public static void Begin()
        {
            m_Batch.Begin();
        }

        public ccColor4B DefaultColor
        {
            get { return m_Color; }
            set { m_Color = value; }
        }

        public static void End()
        {
            m_Batch.End();
        }

        public static void DrawPoint(CCPoint point)
        {
            DrawPoint(point, m_PointSize, m_Color);
        }

        public static void DrawPoint(CCPoint point, float size)
        {
            DrawPoint(point, size, m_Color);
        }

        public static void DrawPoint(CCPoint p, float size, ccColor4B color)
        {
            var verts = new CCPoint[4];

            float hs = size / 2.0f;

            verts[0] = p + new CCPoint(-hs, -hs);
            verts[1] = p + new CCPoint(hs, -hs);
            verts[2] = p + new CCPoint(hs, hs);
            verts[3] = p + new CCPoint(-hs, hs);

            DrawPoly(verts, 4, false, true, color);
        }

        public static void DrawPoints(CCPoint[] points, float size, ccColor4B color)
        {
            DrawPoints(points, points.Length, size, color);
        }

        public static void DrawPoints(CCPoint[] points, int numberOfPoints, float size, ccColor4B color)
        {
            for (int i = 0; i < numberOfPoints; i++)
            {
                DrawPoint(points[i], size, color);
            }
        }

        public static void DrawLine(CCPoint origin, CCPoint destination, ccColor4B color)
        {
            float factor = CCDirector.SharedDirector.ContentScaleFactor;

            var c = new Color(color.r, color.g, color.b, color.a);

            m_Batch.AddVertex(new Vector2(origin.x * factor, origin.y * factor), c, PrimitiveType.LineList);
            m_Batch.AddVertex(new Vector2(destination.x * factor, destination.y * factor), c, PrimitiveType.LineList);
        }

        /// <summary>
        /// draws a poligon given a pointer to CCPoint coordiantes and the number of vertices measured in points.
        /// The polygon can be closed or open
        /// </summary>
        public static void DrawPoly(CCPoint[] vertices, int numOfVertices, bool closePolygon, ccColor4B color)
        {
            DrawPoly(vertices, numOfVertices, closePolygon, false, color);
        }

        /// <summary>
        /// draws a poligon given a pointer to CCPoint coordiantes and the number of vertices measured in points.
        /// The polygon can be closed or open and optionally filled with current GL color
        /// </summary>
        public static void DrawPoly(CCPoint[] vertices, int numOfVertices, bool closePolygon, bool fill, ccColor4B color)
        {
            var c = new Color(color.r, color.g, color.b, color.a);

            if (fill)
            {
                for (int i = 1; i < numOfVertices - 1; i++)
                {
                    m_Batch.AddVertex(new Vector2(vertices[0].x, vertices[0].y), c, PrimitiveType.TriangleList);
                    m_Batch.AddVertex(new Vector2(vertices[i].x, vertices[i].y), c, PrimitiveType.TriangleList);
                    m_Batch.AddVertex(new Vector2(vertices[i + 1].x, vertices[i + 1].y), c, PrimitiveType.TriangleList);
                }
            }
            else
            {
                for (int i = 0; i < numOfVertices - 1; i++)
                {
                    m_Batch.AddVertex(new Vector2(vertices[i].x, vertices[i].y), c, PrimitiveType.LineList);
                    m_Batch.AddVertex(new Vector2(vertices[i + 1].x, vertices[i + 1].y), c, PrimitiveType.LineList);
                }

                if (closePolygon)
                {
                    m_Batch.AddVertex(new Vector2(vertices[numOfVertices - 1].x, vertices[numOfVertices - 1].y), c, PrimitiveType.LineList);
                    m_Batch.AddVertex(new Vector2(vertices[0].x, vertices[0].y), c, PrimitiveType.LineList);
                }
            }
        }

        public static void DrawSolidPoly(CCPoint[] vertices, int count, ccColor4B color)
        {
            DrawSolidPoly(vertices, count, color, false);
        }

        public static void DrawSolidPoly(CCPoint[] vertices, int count, ccColor4B color, bool outline)
        {
            if (count == 2)
            {
                DrawPoly(vertices, count, false, color);
                return;
            }

            var colorFill = new Color(color.r, color.g, color.b, color.a);

            colorFill = colorFill * (outline ? 0.5f : 1.0f);

            for (int i = 1; i < count - 1; i++)
            {
                m_Batch.AddVertex(new Vector2(vertices[0].x, vertices[0].y), colorFill, PrimitiveType.TriangleList);
                m_Batch.AddVertex(new Vector2(vertices[i].x, vertices[i].y), colorFill, PrimitiveType.TriangleList);
                m_Batch.AddVertex(new Vector2(vertices[i + 1].x, vertices[i + 1].y), colorFill, PrimitiveType.TriangleList);
            }

            if (outline)
            {
                DrawPoly(vertices, count, true, color);
            }
        }

        public static void DrawCircle(CCPoint center, float radius, float angle, int segments, bool drawLineToCenter, ccColor4B color)
        {
            float increment = MathHelper.Pi * 2.0f / segments;
            double theta = 0.0;

            CCPoint v1;
            CCPoint v2 = CCPoint.Zero;

            for (int i = 0; i < segments; i++)
            {
                v1 = center + new CCPoint((float) Math.Cos(theta), (float) Math.Sin(theta)) * radius;
                v2 = center + new CCPoint((float) Math.Cos(theta + increment), (float) Math.Sin(theta + increment)) * radius;

                DrawLine(v1, v2, color);

                theta += increment;
            }

            if (drawLineToCenter)
            {
                DrawLine(center, v2, color);
            }
        }

        public static void DrawQuadBezier(CCPoint origin, CCPoint control, CCPoint destination, int segments, ccColor4B color)
        {
            var vertices = new VertexPositionColor[segments + 1];
            float factor = CCDirector.SharedDirector.ContentScaleFactor;

            float t = 0.0f;
            for (int i = 0; i < segments; i++)
            {
                float x = (float) Math.Pow(1 - t, 2) * origin.x + 2.0f * (1 - t) * t * control.x + t * t * destination.x;
                float y = (float) Math.Pow(1 - t, 2) * origin.y + 2.0f * (1 - t) * t * control.y + t * t * destination.y;
                vertices[i] = new VertexPositionColor();
                vertices[i].Position = new Vector3(x * factor, y * factor, 0);
                vertices[i].Color = new Color(color.r, color.g, color.b, color.a);
                t += 1.0f / segments;
            }
            vertices[segments] = new VertexPositionColor
                {
                    Position = new Vector3(destination.x * factor, destination.y * factor, 0),
                    Color = new Color(color.r, color.g, color.b, color.a),
                };

            BasicEffect basicEffect = DrawManager.PrimitiveEffect;
            basicEffect.Projection = DrawManager.ProjectionMatrix;
            basicEffect.View = DrawManager.ViewMatrix;
            basicEffect.World = DrawManager.WorldMatrix;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                basicEffect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, vertices, 0, segments);
            }
        }

        /// <summary>
        /// draws a cubic bezier path
        /// @since v0.8
        /// </summary>
        public static void DrawCubicBezier(CCPoint origin, CCPoint control1, CCPoint control2, CCPoint destination, int segments, ccColor4B color)
        {
            var vertices = new VertexPositionColor[segments + 1];
            float factor = CCDirector.SharedDirector.ContentScaleFactor;

            float t = 0;
            for (int i = 0; i < segments; ++i)
            {
                float x = (float) Math.Pow(1 - t, 3) * origin.x + 3.0f * (float) Math.Pow(1 - t, 2) * t * control1.x +
                          3.0f * (1 - t) * t * t * control2.x + t * t * t * destination.x;
                float y = (float) Math.Pow(1 - t, 3) * origin.y + 3.0f * (float) Math.Pow(1 - t, 2) * t * control1.y +
                          3.0f * (1 - t) * t * t * control2.y + t * t * t * destination.y;
                vertices[i] = new VertexPositionColor();
                vertices[i].Position = new Vector3(x * factor, y * factor, 0);
                vertices[i].Color = new Color(color.r, color.g, color.b, color.a);
                t += 1.0f / segments;
            }
            vertices[segments] = new VertexPositionColor
                {
                    Color = new Color(color.r, color.g, color.b, color.a),
                    Position = new Vector3(destination.x * factor, destination.y * factor, 0)
                };

            BasicEffect basicEffect = DrawManager.PrimitiveEffect;
            basicEffect.Projection = DrawManager.ProjectionMatrix;
            basicEffect.View = DrawManager.ViewMatrix;
            basicEffect.World = DrawManager.WorldMatrix;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                basicEffect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, vertices, 0, segments);
            }
        }

        public static void DrawCatmullRom(List<CCPoint> points, int segments)
        {
            DrawCardinalSpline(points, 0.5f, segments);
        }

        public static void DrawCardinalSpline(List<CCPoint> config, float tension, int segments)
        {
            var vertices = new VertexPositionColor[segments + 1];

            int p;
            float lt;
            float deltaT = 1.0f / config.Count;

            for (int i = 0; i < segments + 1; i++)
            {
                float dt = (float) i / segments;

                // border
                if (dt == 1)
                {
                    p = config.Count - 1;
                    lt = 1;
                }
                else
                {
                    p = (int) (dt / deltaT);
                    lt = (dt - deltaT * p) / deltaT;
                }

                // Interpolate
                // Interpolate    
                int c = config.Count - 1;
                CCPoint pp0 = config[Math.Min(c, Math.Max(p - 1, 0))];
                CCPoint pp1 = config[Math.Min(c, Math.Max(p + 0, 0))];
                CCPoint pp2 = config[Math.Min(c, Math.Max(p + 1, 0))];
                CCPoint pp3 = config[Math.Min(c, Math.Max(p + 2, 0))];

                CCPoint newPos = Spline.ccCardinalSplineAt(pp0, pp1, pp2, pp3, tension, lt);

                vertices[i].Position.X = newPos.x;
                vertices[i].Position.Y = newPos.y;
                vertices[i].Color = Color.White;
            }

            BasicEffect basicEffect = DrawManager.PrimitiveEffect;
            basicEffect.Projection = DrawManager.ProjectionMatrix;
            basicEffect.View = DrawManager.ViewMatrix;
            basicEffect.World = DrawManager.WorldMatrix;
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                basicEffect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, vertices, 0, segments);
            }
        }
    }
}