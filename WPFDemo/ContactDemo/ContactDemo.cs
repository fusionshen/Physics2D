﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Physics2D;
using Physics2D.Collision;
using Physics2D.Collision.Basic;
using Physics2D.Common;
using Physics2D.Object;
using Physics2D.Force;
using Physics2D.Force.Zones;
using Physics2D.Factories;

using WPFDemo.Graphic;
using WPFDemo.ContactDemo;
using Physics2D.Collision.Shapes;

namespace WPFDemo.ContactDemo
{
    class ContactDemo : PhysicsGraphic
    {
        #region 私有字段
        /// <summary>
        /// 钢珠列表
        /// </summary>
        private readonly List<Ball> _ballList = new List<Ball>();
        #endregion

        #region 构造方法
        public ContactDemo(Image image)
            : base(image)
        {
            Settings.ContactIteration = 1;

            const int num = 5;

            for(int i = 0; i < num; i++)
            {
                var fB = PhysicsWorld.CreateFixedParticle((new Vector2D(160 + 40 * i, 0)).ToSimUnits());
                var pB = PhysicsWorld.CreateParticle((new Vector2D(160 + 40 * i, 200)).ToSimUnits(), new Vector2D(0, 0), 2);
                
                var ball = new Ball
                {
                    FixedParticle = fB,
                    Particle = pB,
                    R = 20
                };

                // 为质体绑定形状
                ball.Particle.BindShape(new Circle(ball.R.ToSimUnits()));

                PhysicsWorld.CreateRope(200.ToSimUnits(), 0, fB, pB);
                DrawQueue.Add(ball);
                _ballList.Add(ball);
            }

            // 增加重力和空气阻力
            PhysicsWorld.CreateGlobalZone(new ParticleGravity(new Vector2D(0, 40)));
            PhysicsWorld.CreateParticle(Vector2D.Zero, new Vector2D(1, 0), 1);
            Slot = 1 / 120.0;

            Start = true;
        }
        #endregion

        #region 实现PhysicsGraphic
        protected override void UpdatePhysics(double duration)
        {
            PhysicsWorld.Update(duration);
        }
        #endregion

        #region 鼠标事件响应
        public void Fire()
        {
            _ballList[0].Particle.Velocity = new Vector2D(-10, 0);
        }
        #endregion
    }
}
