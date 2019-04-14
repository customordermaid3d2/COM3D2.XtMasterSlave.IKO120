#define IKO118

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static ExtensionMethods.MyExtensions;
using CM3D2.XtMasterSlave.Plugin;
using UnityEngine;
using System.Reflection;

namespace XtMasterSlave_IK_XDLL
{
#if IKO118
    public class IkpInst : IkInst
    {
        public static bool boAnime = false; //?
        public static bool IKBend = false;  //

        public override bool IsNewPointIK(Maid m, string hand = "右手")
        {
            var ikP = m.body0.IKCtrl.GetIKData(hand, IKBend).GetIKParam(IKCtrlData.IKAttachType.Point);
            return (ikP.MyType == IKCtrlData.IKAttachType.NewPoint);
        }

        public override object GetIkPoint(TBody body, string hand = "右手")
        {
#if DEBUG
            if (Input.GetKey(KeyCode.Space))
            {
                IKBend = true;
                Console.WriteLine("IKBend: ON");
            }
            else
            {
                IKBend = false;
            }
#endif
            var obj = body.IKCtrl.GetIKData(hand, IKBend).GetIKParam(IKCtrlData.IKAttachType.Point);
            if (obj == null)
                obj = body.IKCtrl.GetIKData(hand, IKBend).GetIKParam(IKCtrlData.IKAttachType.NewPoint);

            return obj;
        }

        public override object GetIkCtrl(Maid maid)
        {
            return maid.IKCtrl;
        }

        public override object GetIkCtrlPoint(TBody body, string hand = "右手")
        {
#if DEBUG
            if (Input.GetKey(KeyCode.Space))
            {
                IKBend = true;
                Console.WriteLine("IKBend: ON");
            }
            else
            {
                IKBend = false;
            }
#endif
            var obj = body.IKCtrl.GetIKData(hand, IKBend);
            return obj;
        }

        private IKCtrlData.IKAttachType GetDefType(XtMasterSlave.MsLinkConfig mscfg)
        {
            if (mscfg.doIK159NewPointToDef)
            {
                return IKCtrlData.IKAttachType.NewPoint;
            }
            else
            {
                return IKCtrlData.IKAttachType.Point;
            }
        }

        public override void IkClear(Maid tgt, XtMasterSlave.MsLinkConfig mscfg)
        {
            List<string> listHand = new List<string> { "右手", "左手" };
            IkClear(tgt, listHand, mscfg);
        }

        //public override void IkClear(Maid tgt, List<string> listHand, XtMasterSlave.MsLinkConfig mscfg, IKCtrlData.IKAttachType IkType = (IKCtrlData.IKAttachType)(-1))
        public override void IkClear(Maid tgt, List<string> listHand, XtMasterSlave.MsLinkConfig mscfg, int IkType = (-1))
        {
            List<IKCtrlData.IKAttachType> listTypes = new List<IKCtrlData.IKAttachType>
                                    { IKCtrlData.IKAttachType.NewPoint, IKCtrlData.IKAttachType.Rotate };

            listHand.ToList().ForEach(h =>
            {
                var ctrl = tgt.body0.IKCtrl.GetIKData(h, IKBend);
                listTypes.ForEach(t =>
                {
                    var iks = ctrl.GetIKParam(t);

                    if (IkXT.IsIkCtrlO117)
                    {
                        //ctrl.SetIKSetting(t, false, null, -1, string.Empty, null, null, Vector3.zero, false, 0f);
                        ctrl.SetIKSetting(t, false, null, -1, string.Empty, null, null, Vector3.zero, false);
                        //iks.SetIKSetting(null, -1, string.Empty, null, null, Vector3.zero, false, 0f);
                        //ctrl.Detach(t, 0f);
                        ctrl.Detach(t);
                    }
                    else
                    {
                        iks.TgtMaid = null;
                        iks.Tgt_AttachSlot = -1;
                        iks.Tgt_AttachName = string.Empty;
                        iks.Target = null;
                        iks.AxisTgt = null;
                        iks.TgtOffset = Vector3.zero;
                        //iks.IsTgtAxis
                    }

                    if (iks.MyType != IKCtrlData.IKAttachType.Rotate)
                    {
                        if (IkType >= 0 && IkType != (int)IKCtrlData.IKAttachType.Rotate
                                && Enum.IsDefined(typeof(IKCtrlData.IKAttachType), IkType))
                        {
                            iks.ChangePointType((IKCtrlData.IKAttachType)IkType);
                        }
                        else
                        {
                            if (mscfg != null)
                                iks.ChangePointType(GetDefType(mscfg));/*fix v5.0
                            else
                                iks.ChangePointType(IKCtrlData.IKAttachType.NewPoint);*/
                        }
                    }
                });
            });
        }

        public override void CopyHandIK(Maid master, Maid slave, XtMasterSlave.v3Offsets[] v3ofs, int num_)
        {
            List<string> listHand = new List<string> { "右手", "左手" };
            List<IKCtrlData.IKAttachType> listTypes = new List<IKCtrlData.IKAttachType>
                                    { IKCtrlData.IKAttachType.NewPoint, IKCtrlData.IKAttachType.Rotate };

            listHand.ToList().ForEach(h =>
            {
                var ikcm = master.body0.IKCtrl.GetIKData(h, IKBend);
                var ikcs = slave.body0.IKCtrl.GetIKData(h, IKBend);
                listTypes.ForEach(t =>
                {
                    var ikm = ikcm.GetIKParam(t);
                    var iks = ikcs.GetIKParam(t);

                    if (!(string.IsNullOrEmpty(ikm.Tgt_AttachName) && ikm.Target == null))
                    {
                        //Console.WriteLine("{0} {1} -> {2} {3} {4}", h, t, ikm.MyType, ikm.Tgt_AttachName, ikm.Target);

                        if (iks.MyType != IKCtrlData.IKAttachType.Rotate)
                        {
                            if (ikm.MyType != IKCtrlData.IKAttachType.Rotate)
                            {
                                iks.ChangePointType(ikm.MyType);
                            }
                        }

                        float fixAngle(float angle)
                        {
                            while (Mathf.Abs(angle) > 360f)
                            {
                                angle = ((!(angle < 0f)) ? (angle - 360f) : (angle + 360f));
                            }
                            return angle;
                        }

                        if (IkXT.IsIkCtrlO117)
                        {
                            ikcs.SetIKSetting(t, false, ikm.TgtMaid, ikm.Tgt_AttachSlot, ikm.Tgt_AttachName, ikm.AxisTgt, ikm.Target, ikm.TgtOffset, ikm.DoAnimation);
                            //iks.SetIKSetting(ikm.TgtMaid, ikm.Tgt_AttachSlot, ikm.Tgt_AttachName, ikm.AxisTgt, ikm.Target, ikm.TgtOffset, ikm.DoAnimation, ikm.BlendTime);
                        }
                        else
                        {
                            iks.TgtMaid = ikm.TgtMaid;
                            iks.Tgt_AttachSlot = ikm.Tgt_AttachSlot;
                            iks.Tgt_AttachName = ikm.Tgt_AttachName;
                            iks.Target = ikm.Target;
                            iks.AxisTgt = ikm.AxisTgt;
                        }

                        if (iks.IsPointAttach)
                        {
                            iks.TgtOffset = ikm.TgtOffset;
                            if (h == "右手")
                                iks.TgtOffset += v3ofs[num_].v3HandROffset;
                            else
                                iks.TgtOffset += v3ofs[num_].v3HandLOffset;
                        }
                        else
                        {
                            Vector3 v3rot = Vector3.zero;
                            if (h == "右手")
                                v3rot = v3ofs[num_].v3HandROffsetRot;
                            else
                                v3rot = v3ofs[num_].v3HandLOffsetRot;

                            iks.TgtOffset.x = fixAngle(ikm.TgtOffset.x + v3rot.x);
                            iks.TgtOffset.y = fixAngle(ikm.TgtOffset.y + v3rot.y);
                            iks.TgtOffset.z = fixAngle(ikm.TgtOffset.z + v3rot.z);
                        }
                    }

                });
            });

            //needInit = true;
        }

        public override void SetHandIKRotate(string handName, Maid master, Maid slave, string boneTgtname, Vector3 v3HandLOffsetRot)
        {
            //slave.IKTargetToBone(handName, master, boneTgtname, v3HandLOffsetRot, IKCtrlData.IKAttachType.Rotate, false, 0f, boAnime, false);
            slave.IKTargetToBone(handName, master, boneTgtname, v3HandLOffsetRot, IKCtrlData.IKAttachType.Rotate, false, boAnime, false);
        }

        public override void SetHandIKTarget(XtMasterSlave.MsLinkConfig mscfg, string handName, Maid master, Maid slave, int slot_no, string attach_name, Transform target, Vector3 v3HandLOffset)
        {
            /*if (needInit)
            {
                needInit = false;
                if (mscfg.doIK159NewPointToDef)
                    IKInit(slave, mscfg);
#if DEBUG
                else
                    IKInit4OldPoint(slave);
#endif
            }*/

            //slave.IKCtrl.GetIKData(handName, IKBend).SetIKSetting(GetDefType(mscfg), false, master, slot_no, attach_name, null, target, v3HandLOffset, boAnime, 0f);
            slave.IKCtrl.GetIKData(handName, IKBend).SetIKSetting(GetDefType(mscfg), false, master, slot_no, attach_name, null, target, v3HandLOffset, boAnime);

            BipedIKCtrlData ikdata = slave.IKCtrl.GetIKData<BipedIKCtrlData>(handName, IKBend);
            ikdata.CorrectType = BipedIKCtrlData.BorderCorrectType.Bone;
        }

        bool needInit = true;
        static string bodytgt = "dummyBodyTgtXt";
        //RootMotion.FinalIK.FullBodyBipedIK m_FullbodyIK;
#if DEBUG
        /*GameObject goBodyTgt = new GameObject("dummyBodyTgtXt");
        RootMotion.FinalIK.FullBodyBipedIK m_FullbodyIK;
        GameObject goTgtSR = new GameObject("xtTgtSR");
        GameObject goTgtSL = new GameObject("xtTgtSL");*/
#endif

        Dictionary<Maid, string> lastAnimeFNs = new Dictionary<Maid, string>();
        private void IKInit(Maid slave, XtMasterSlave.MsLinks ms, XtMasterSlave.MsLinkConfig mscfg)
        {
            var fik = slave.body0.IKCtrl.GetNonPublicField<RootMotion.FinalIK.FullBodyBipedIK>("m_FullbodyIK");
            RootMotion.FinalIK.FullBodyBipedIK FullbodyIK = fik;
            var solver = FullbodyIK.solver;
            string[] tgtlist = new string[] { "IKTarget", "BendBone", "ChainRootBone" };

            bool animStop = true; //モーション停止中
            if (ms.doMasterSlave && !mscfg.doStackSlave_PosSyncMode)
            {
                animStop = false;
            }
            else
            {
                Animation anim = slave.body0.m_Bones.GetComponent<Animation>();
                animStop = !anim.isPlaying;
            }

            if (!lastAnimeFNs.ContainsKey(slave) || slave.body0.LastAnimeFN != lastAnimeFNs[slave])
            {
                lastAnimeFNs[slave] = slave.body0.LastAnimeFN;
                animStop = false;
            }

            solver.spineStiffness = 1f;      //背骨の硬さ
            solver.pullBodyVertical = 0.5f;  //ボディエフェクター位置補正
            solver.pullBodyHorizontal = 0f;
            solver.spineMapping.twistWeight = 0f;

            foreach (var e in solver.effectors)
            {
                if (solver.leftHandEffector == e || solver.rightHandEffector == e)
                {
                    // 手のIKは本体側に任せる
                    continue;
                }

                bool donotPin = false;

                if (animStop || donotPin)
                {
                    e.positionWeight = 1f;
                    e.rotationWeight = 0f;
                }
                else
                {
                    e.PinToBone(1f, 0f);
                }

                var tgtname = e.target.gameObject.name;

                if (donotPin)
                {
                    continue;
                }
                else
                {
                    if (tgtlist.Contains(tgtname))
                    {
                        // COM3D2標準ターゲット
                        e.target.transform.position = e.bone.position;
                        e.target.transform.rotation = e.bone.rotation;
                    }
                }
            }

            solver.rightShoulderEffector.positionWeight = 0.95f;
            solver.leftShoulderEffector.positionWeight = 0.95f;

            solver.bodyEffector.rotationWeight = 1f;

            solver.rightThighEffector.positionWeight = 0.95f;
            solver.leftThighEffector.positionWeight = 0.95f;

            if (mscfg != null && mscfg.doFinalIKShoulderMove)
            {
                solver.rightShoulderEffector.positionWeight = 0f;
                solver.leftShoulderEffector.positionWeight = 0f;
            }
            if (mscfg != null && mscfg.doFinalIKThighMove)
            {
                solver.bodyEffector.rotationWeight = 0f;

                solver.rightThighEffector.positionWeight = 0f;
                solver.leftThighEffector.positionWeight = 0f;
            }

            foreach (var m in solver.limbMappings)
            {
                m.weight = 1f;
                m.maintainRotationWeight = 0f;
            }

            if (mscfg != null)
            {
                solver.rightLegMapping.weight = mscfg.fFinalIKLegWeight; //0.5f;
                solver.leftLegMapping.weight = mscfg.fFinalIKLegWeight; //0.5f;
            }
            solver.rightLegMapping.maintainRotationWeight = 1f;
            solver.leftLegMapping.maintainRotationWeight = 1f;
        }

        #region for TEST
#if DEBUG
        private void IKInit4OldPoint(Maid slave)
        {
            var fik = slave.body0.IKCtrl.GetNonPublicField<RootMotion.FinalIK.FullBodyBipedIK>("m_FullbodyIK");
            RootMotion.FinalIK.FullBodyBipedIK FullbodyIK = fik;
            var solver = FullbodyIK.solver;
            string[] tgtlist = new string[] { "IKTarget", "BendBone", "ChainRootBone" };

            solver.spineStiffness = 1f;      //背骨の硬さ
            solver.pullBodyVertical = 0f;  //ボディエフェクター位置補正
            solver.pullBodyHorizontal = 0f;
            solver.spineMapping.twistWeight = 0f;

            foreach (var e in solver.effectors)
            {
                e.PinToBone(1f, 1f);

                var tgtname = e.target.gameObject.name;

                if (tgtlist.Contains(tgtname))
                {
                    // COM3D2標準ターゲット
                    e.target.transform.position = e.bone.position;
                    e.target.transform.rotation = e.bone.rotation;
                }
            }
            
            foreach(var m in solver.limbMappings)
            {
                m.weight = 0f;
                m.maintainRotationWeight = 1f;
            }
        }

        private void IKInit2(Maid slave)
        {
            var fik = slave.body0.IKCtrl.GetNonPublicField<RootMotion.FinalIK.FullBodyBipedIK>("m_FullbodyIK");
            RootMotion.FinalIK.FullBodyBipedIK FullbodyIK = fik;
            var solver = FullbodyIK.solver;
            string[] tgtlist = new string[] { "IKTarget", "BendBone", "ChainRootBone" };
            string[] bendlist = new string[] { "BendBone", };
            /*
#if DEBUG
            solver.spineStiffness = 1f;      //背骨の硬さ
            //solver.rightLegMapping.weight = 0f;
            //solver.leftLegMapping.weight = 0f;
            if (!solver.bodyEffector.target || solver.bodyEffector.target.gameObject.name != bodytgt)
                solver.bodyEffector.target = new GameObject(bodytgt).transform;
            solver.bodyEffector.positionWeight = 1f;
            solver.bodyEffector.rotationWeight = 1f;
            solver.rightShoulderEffector.positionWeight = 0.95f;
            solver.leftShoulderEffector.positionWeight = 0.95f;
            solver.rightShoulderEffector.rotationWeight = 0.5f;
            solver.leftShoulderEffector.rotationWeight = 0.5f;

            solver.leftThighEffector.positionWeight = 1f;
            solver.leftThighEffector.rotationWeight = 0.5f;
            solver.rightThighEffector.positionWeight = 1f;
            solver.rightThighEffector.rotationWeight = 0.5f;
            solver.leftFootEffector.positionWeight = 1.0f;
            solver.rightFootEffector.positionWeight = 1.0f;

            if (!solver.rightThighEffector.target || solver.rightThighEffector.target.gameObject.name != bodytgt)
                solver.rightThighEffector.target = new GameObject(bodytgt).transform;
            if (!solver.leftThighEffector.target || solver.leftThighEffector.target.gameObject.name != bodytgt)
                solver.leftThighEffector.target = new GameObject(bodytgt).transform;

            if (!solver.rightFootEffector.target || solver.rightFootEffector.target.gameObject.name != bodytgt)
                solver.rightFootEffector.target = new GameObject(bodytgt).transform;
            if (!solver.leftFootEffector.target || solver.leftFootEffector.target.gameObject.name != bodytgt)
                solver.leftFootEffector.target = new GameObject(bodytgt).transform;

            solver.rightLegMapping.weight = 0.2f;
            solver.leftLegMapping.weight = 0.2f;
            //Sync(solver.leftArmChain.bendConstraint.bendGoal.transform, solver.leftArmMapping.bone2);
            //Sync(solver.rightArmChain.bendConstraint.bendGoal.transform, solver.rightArmMapping.bone2);
            foreach (var e in solver.effectors)
            {
                Sync(e);
                var tgtname = e.target.gameObject.name;

                if (tgtlist.Contains(tgtname) || tgtname == bodytgt)
                {
                    // COM3D2標準ターゲット
                    e.target.transform.position = e.bone.position;
                    e.target.transform.rotation = e.bone.rotation;
                }
                else if (bendlist.Contains(tgtname))
                {
                    // COM3D2標準ターゲット
                    e.target.transform.position = e.bone.position;
                    e.target.transform.rotation = e.bone.rotation;
                }
            }
            return;
#endif

            */
            solver.spineStiffness = 1f;      //背骨の硬さ
            solver.pullBodyVertical = 0.5f;  //ボディエフェクター位置補正
            solver.pullBodyHorizontal = 0f;

            foreach (var e in solver.effectors)
            {

                e.PinToBone(1f, 0f);
#if DEBUG
                e.PinToBone(1f, 1f);
#endif
                var tgtname = e.target.gameObject.name;

                if (tgtlist.Contains(tgtname))
                {
                    // COM3D2標準ターゲット
                    e.target.transform.position = e.bone.position;
                    e.target.transform.rotation = e.bone.rotation;
                }
            }
            solver.rightShoulderEffector.positionWeight = 0.95f;
            solver.leftShoulderEffector.positionWeight = 0.95f;
            solver.bodyEffector.rotationWeight = 1f;

            solver.rightLegMapping.maintainRotationWeight = 0f;
            solver.leftLegMapping.maintainRotationWeight = 0f;
            solver.rightLegMapping.weight = 0f;
            solver.leftLegMapping.weight = 0f;

            solver.rightArmMapping.maintainRotationWeight = 0f;
            solver.leftArmMapping.maintainRotationWeight = 0f;
            solver.rightArmMapping.weight = 1f;
            solver.leftArmMapping.weight = 1f;
        }

        private void IKInitTest(Maid slave, string handName)
        {
            var fik = slave.body0.IKCtrl.GetNonPublicField<RootMotion.FinalIK.FullBodyBipedIK>("m_FullbodyIK");
            RootMotion.FinalIK.FullBodyBipedIK FullbodyIK = fik;

            var solver = FullbodyIK.solver;
            if (handName.Contains("右"))
            {
                //solver.rightLegMapping.weight = 0f;
                //solver.spineMapping.twistWeight = 0f;

                //m_FullbodyIK.references.root = slave.gameObject.transform;
                //solver.SetToReferences(FullbodyIK.references, null);
                solver.spineStiffness = 1f;
                solver.pullBodyVertical = 0f;
                solver.pullBodyHorizontal = 0f;

                if (Input.GetKey(KeyCode.LeftAlt))
                {
                    solver.spineStiffness = 0f;
                    solver.pullBodyVertical = 1f;
                }
                foreach (var e in solver.effectors)
                {
                    if (Input.GetKey(KeyCode.RightAlt))
                        e.PinToBone(0.25f, 1f);
                    else if (Input.GetKey(KeyCode.RightShift))
                        e.PinToBone(0.5f, 0f);
                    else if (Input.GetKey(KeyCode.RightControl))
                        e.PinToBone(0f, 1f);
                    else
                        e.PinToBone(1f, 1f);
                }
                solver.rightLegMapping.maintainRotationWeight = 1f;
                solver.leftLegMapping.maintainRotationWeight = 1f;

                if (Input.GetKey(KeyCode.Space))
                    solver.rightLegMapping.weight = 1f;
                else if (Input.GetKey(KeyCode.Keypad0))
                    solver.rightLegMapping.weight = 0f;
                else
                    solver.rightLegMapping.weight = 0.5f;

                /*Sync(solver.bodyEffector);
                Sync(solver.rightFootEffector);
                Sync(solver.rightThighEffector);
                Sync(solver.rightShoulderEffector);
                WeightZero(solver.rightFootEffector);
                WeightZero(solver.rightThighEffector);
                WeightZero(solver.rightShoulderEffector);*/
            }
            else
            {
                solver.leftLegMapping.maintainRotationWeight = 1f;
                if (Input.GetKey(KeyCode.Space))
                    solver.leftLegMapping.weight = 1f;
                else if (Input.GetKey(KeyCode.Keypad0))
                    solver.leftLegMapping.weight = 0f;
                else
                    solver.leftLegMapping.weight = 0.5f;

                /*Sync(solver.leftFootEffector);
                Sync(solver.leftThighEffector);
                Sync(solver.leftShoulderEffector);
                WeightZero(solver.leftFootEffector);
                WeightZero(solver.leftThighEffector);
                WeightZero(solver.leftShoulderEffector);*/
            }
        }
#endif
        #endregion

        private static void Sync(Transform tr1, Transform tr2)
        {
            tr1.position = tr2.position;
            tr1.rotation = tr2.rotation;
        }
        private static void Sync(RootMotion.FinalIK.IKEffector eff)
        {
            eff.position = eff.bone.position;
            eff.rotation = eff.bone.rotation;
            /*eff.target.position = eff.bone.position;
            eff.target.rotation = eff.bone.rotation;
            eff.PinToBone(1f, 1f);*/
        }
        private static void WeightZero(RootMotion.FinalIK.IKEffector eff)
        {
            eff.positionWeight = 0f;
            eff.rotationWeight = 0f;
        }

        public override object GetIKCmo(TBody body, string hand = "右手")
        {
            return body.IKCtrl.GetIKData(hand, IKBend).IKCmo;

            /*
            if (hand == "右手")
                return body.IKCtrl.GetIKData("右手").IKCmo;
            else
                return body.IKCtrl.GetIKData("左手").IKCmo;
                */
        }

        public override bool IKUpdate(TBody body)
        {
            body.IKCtrl.IKUpdate();
            return true;
        }

        public override bool GetIKCmoPosRot(TBody body, out Vector3 pos, out Quaternion rot, string hand = "右手")
        {
            var ctrl = body.IKCtrl.GetIKData(hand, IKBend);
            bool proc = false;

            pos = Vector3.zero;
            rot = Quaternion.identity;

            var data = ctrl.GetIKParam(IKCtrlData.IKAttachType.Point);
            if (data.Target != null)
            {
                pos = data.Target.position;
                rot = data.Target.rotation;
                proc = true;
            }
            else if (data.Tgt_AttachName != string.Empty)
            {
                if (data.TgtMaid != null && data.TgtMaid.body0 != null && data.Tgt_AttachSlot >= 0 && data.TgtMaid.body0.goSlot[data.Tgt_AttachSlot].morph != null)
                {
                    Vector3 vector;
                    data.TgtMaid.body0.goSlot[data.Tgt_AttachSlot].morph.GetAttachPoint(data.Tgt_AttachName, out pos, out rot, out vector, false);
                    proc = true;
                }
                else
                {
                    data.Tgt_AttachName = string.Empty;
                }
            }

            return proc;
        }

        public override bool IKCmoUpdate(TBody body, Transform trh, Vector3 offset, string hand = "右手")
        {
            var ctrl = body.IKCtrl.GetIKData(hand, IKBend);
            /*ctrl.MyIKCtrl.GetType().GetProperty("IsUpdateEnd").SetValue(ctrl.MyIKCtrl, true, null);
            ctrl.MyIKCtrl.IsUpdateLate = false;
            ctrl.ApplyIKSetting();
            */

            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;
            bool proc = GetIKCmoPosRot(body, out pos, out rot, hand);

            if (proc)
            {
                ctrl.IKCmo.Porc(trh.parent.parent, trh.parent, trh, pos, rot * offset, ctrl);
                return true;
            }
            return false;
        }

        public override bool UpdateFinalIK(Maid maid, XtMasterSlave.MsLinks ms, XtMasterSlave.MsLinkConfig mscfg)
        {
            if (!maid || !maid.body0)
                return false;

            needInit = false;
            if (mscfg.doIK159NewPointToDef)
                IKInit(maid, ms, mscfg);
#if DEBUG
            else
                IKInit4OldPoint(maid);
#endif
            return true; // 実行できたか
        }
    }
#endif
}

public static class Extentions
{
    public static T GetNonPublicField<T>(this object obj, string name)
    {
        var ret = obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj);

        if (ret is T)
            return (T)ret;
        return default(T);
    }
}