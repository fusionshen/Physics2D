﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Physics2D.Collision;
using Physics2D.Object;
using Physics2D.Collision.Shapes;
using System.Diagnostics;

namespace Physics2D.Core
{
    public sealed class ContactRegistry
    {
        #region 私有部分
        /// <summary>
        /// 物理世界物体列表的引用
        /// </summary>
        private readonly HashSet<PhysicsObject> _objects;

        /// <summary>
        /// 质体碰撞发生器集合
        /// </summary>
        private readonly HashSet<ParticleContactGenerator> _generators = new HashSet<ParticleContactGenerator>();

        /// <summary>
        /// 质体碰撞表
        /// </summary>
        private readonly List<ParticleContact> _contactList = new List<ParticleContact>();

        /// <summary>
        /// 碰撞解决器
        /// </summary>
        private readonly ParticleContactResolver _particleContactResolver = new ParticleContactResolver(0);

        /// <summary>
        /// 碰撞计数器
        /// </summary>
        private int _contactCounter = 0;

        /// <summary>
        /// 碰撞类型查询表
        /// </summary>
        private static ContactType[,] _contactTypeMap = new[,]
        {
            {
                ContactType.CircleAndCircle,
                ContactType.CircleAndEdge,
                ContactType.CircleAndBox
            },
            {
                ContactType.CircleAndEdge,
                ContactType.EdgeAndEdge,
                ContactType.EdgeAndBox
            },
            {
                ContactType.CircleAndBox,
                ContactType.EdgeAndBox,
                ContactType.BoxAndBox
            }
        };

        private void DispatchToDetector(ContactType type, Shape sharpA, Shape sharpB)
        {
            Debug.Assert(sharpA.Type <= sharpB.Type);

            ParticleContact contact = null;

            switch(type)
            {
                case ContactType.CircleAndCircle:
                    ParticleCollisionDetector.CircleAndCircle((Circle)sharpA, (Circle)sharpB, out contact);
                    break;
                case ContactType.CircleAndEdge:
                    break;
                case ContactType.CircleAndBox:
                    break;
                case ContactType.EdgeAndEdge:
                    break;
                case ContactType.EdgeAndBox:
                    break;
                case ContactType.BoxAndBox:
                    break;
            }
            if(contact != null)
                AddToContactList(contact);
        }

        /// <summary>
        /// 向碰撞表中添加一个新的碰撞
        /// </summary>
        /// <param name="contact">碰撞信息</param>
        /// <returns>完成添加后若不允许继续添加则返回false，否则返回true</returns>
        private bool AddToContactList(ParticleContact contact)
        {
            if (_contactCounter++ < Settings.MaxContacts)
            {
                _contactList.Add(contact);
                return true;
            }
            return false;
        }
        #endregion

        public ContactRegistry(HashSet<PhysicsObject> objects)
        {
            _objects = objects;
        }

        #region 碰撞发生器的注册与注销
        /// <summary>
        /// 注册碰撞发生器
        /// </summary>
        /// <param name="contactGenerator"></param>
        public void Add(ParticleContactGenerator contactGenerator) => _generators.Add(contactGenerator);

        /// <summary>
        /// 移除碰撞发生器
        /// </summary>
        /// <param name="contactGenerator"></param>
        public void Remove(ParticleContactGenerator contactGenerator) => _generators.Remove(contactGenerator);
        #endregion

        /// <summary>
        /// 进行碰撞检测并解决碰撞
        /// </summary>
        /// <param name="duration"></param>
        public void ResolveContacts(double duration)
        {
            _contactCounter = 0;
            
            for (int i = 0; i < Settings.ContactIteration; i++)
            {
                // 产生碰撞表
                _contactList.Clear();

                // 执行碰撞检测器
                var objects = _objects.Where(obj => obj.Shape.Type != ShapeType.Point).ToList();
                for (int indexA = 0; indexA < objects.Count; indexA++)
                {
                    for(int indexB = indexA + 1; indexB < objects.Count; indexB++)
                    {
                        ShapeType typeA = objects[indexA].Shape.Type;
                        ShapeType typeB = objects[indexB].Shape.Type;

                        if(typeA <= typeB)
                        {
                            DispatchToDetector(_contactTypeMap[(int)typeA, (int)typeB], objects[indexA].Shape, objects[indexB].Shape);
                        }
                        else
                        {
                            DispatchToDetector(_contactTypeMap[(int)typeA, (int)typeB], objects[indexB].Shape, objects[indexA].Shape);
                        }
                    }
                }



                // 执行碰撞发生器
                foreach (var contactGenerator in _generators)
                {
                    foreach (var contact in contactGenerator)
                    {
                        if (!AddToContactList(contact)) goto CONTACT_RESOLVE;
                    }
                }

                // 当不再产生新的碰撞时退出
                if (_contactList.Count == 0) break;
                
                CONTACT_RESOLVE:
                // 解决质体碰撞
                _particleContactResolver.Iterations = _contactList.Count * 2;
                _particleContactResolver.ResolveContacts(_contactList, duration);
            }

        }
    }
}
