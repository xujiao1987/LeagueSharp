using System;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace JayceSharp
{
    class Jayce
    {
        public static Obj_AI_Hero Player = ObjectManager.Player;


        public static Spellbook sBook = Player.Spellbook;

        public static Orbwalking.Orbwalker orbwalker;

        public static SpellDataInst Qdata = sBook.GetSpell(SpellSlot.Q);
        public static SpellDataInst Wdata = sBook.GetSpell(SpellSlot.W);
        public static SpellDataInst Edata = sBook.GetSpell(SpellSlot.E);
        public static SpellDataInst Rdata = sBook.GetSpell(SpellSlot.R);
        public static Spell Q1 = new Spell(SpellSlot.Q, 1250);//Emp 1470
        public static Spell QEmp1 = new Spell(SpellSlot.Q, 1600);//Emp 1470
        public static Spell W1 = new Spell(SpellSlot.W, 0);
        public static Spell E1 = new Spell(SpellSlot.E, 650);
        public static Spell R1 = new Spell(SpellSlot.R, 0);

        public static Spell Q2 = new Spell(SpellSlot.Q, 600);
        public static Spell W2 = new Spell(SpellSlot.W, 285);
        public static Spell E2 = new Spell(SpellSlot.E, 240);
        public static Spell R2 = new Spell(SpellSlot.R, 0);

        public static GameObjectProcessSpellCastEventArgs castEonQ = null;

        public static Obj_SpellMissile myCastedQ = null;

        public static Obj_AI_Hero lockedTarg = null;

        public static Vector3 castQon = new Vector3(0,0,0);

        /* COOLDOWN STUFF */
        public static float[] rangTrueQcd = { 8, 8, 8, 8, 8 };
        public static float[] rangTrueWcd = { 14, 12, 10, 8, 6 };
        public static float[] rangTrueEcd = { 16, 16, 16, 16, 16 };

        public static float[] hamTrueQcd = { 16, 14, 12, 10, 8 };
        public static float[] hamTrueWcd = { 10, 10, 10, 10, 10 };
        public static float[] hamTrueEcd = { 14, 12, 12, 11, 10 };

        public static float rangQCD=0, rangWCD=0, rangECD = 0;
        public static float hamQCD = 0, hamWCD = 0, hamECD = 0;

        public static float rangQCDRem = 0, rangWCDRem = 0, rangECDRem = 0;
        public static float hamQCDRem = 0, hamWCDRem = 0, hamECDRem = 0;


        /* COOLDOWN STUFF END */
        public static bool isHammer = false;

        public static void setSkillShots()
        {
            Q1.SetSkillshot(0.15f, 70f, 1100, true, SkillshotType.SkillshotLine);
            QEmp1.SetSkillshot(0.15f, 70f, 2050, true, SkillshotType.SkillshotLine);
           // QEmp1.SetSkillshot(0.25f, 70f, float.MaxValue, false, SkillshotType.SkillshotLine);
        }


        public static void doCombo(Obj_AI_Hero target)
        {
            if (!isHammer)
            {
                if (castEonQ != null)
                    castEonSpell(target);
                //DO QE combo first
                if (E1.IsReady() && Q1.IsReady() && gotManaFor(true, false, true))
                {

                    PredictionOutput po = QEmp1.GetPrediction(target);
                    if (po.Hitchance == HitChance.High/* && Player.Distance(po.CastPosition) < (QEmp1.Range + target.BoundingRadius)*/)
                    {
                       // Vector3 bPos = Player.ServerPosition - (target.Position - Player.ServerPosition);

                       // Player.IssueOrder(GameObjectOrder.MoveTo, bPos);
                        castQon = po.CastPosition;
                        // shootQE(po.CastPosition);
                        // QEmp1.Cast(po.CastPosition);
                    }

                   // QEmp1.CastIfHitchanceEquals(target, HitChance.High);
                }
                else if (Q1.IsReady() && gotManaFor(true))
                {
                    PredictionOutput po = Q1.GetPrediction(target);
                    if (po.Hitchance >= HitChance.Low/* && Player.Distance(po.CastPosition) < (Q1.Range + target.BoundingRadius)*/)
                    {
                        Q1.Cast(po.CastPosition);
                    }
                }
                else if (W1.IsReady() && gotManaFor(false, true) && targetInRange(getClosestEnem(), 650f))
                {
                    W1.Cast();
                }//and wont die wih 1 AA
                else if (!Q1.IsReady() && !W1.IsReady() && R1.IsReady() && hammerWillKill(target) && hamQCDRem==0 && hamECDRem==0)// will need to add check if other form skills ready
                {
                    R1.Cast();
                }
            }
            else
            {
                if (!Q2.IsReady() && R2.IsReady() && Player.Distance(getClosestEnem()) > 350)
                {
                    R2.Cast();
                }
                if (Q2.IsReady() && gotManaFor(true) && targetInRange(target,Q2.Range) && Player.Distance(target)>300)
                {
                    Q2.Cast(target);
                }
                if (E2.IsReady() && gotManaFor(false, false, true) && targetInRange(target, E2.Range) && shouldIKnockDatMadaFaka(target))
                {
                    E2.Cast(target);
                } 
                if (W2.IsReady() && gotManaFor(false, true) && targetInRange(target, W2.Range))
                {
                    W2.Cast();
                }
               
            }
        }


        public static void doFullDmg(Obj_AI_Hero target)
        {
            if (!isHammer)
            {
                if (castEonQ != null )
                {
                    castEonSpell(target);
                }
                //DO QE combo first
                if (E1.IsReady() && Q1.IsReady() && gotManaFor(true, false, true))
                {
                    PredictionOutput po = QEmp1.GetPrediction(target);
                    if (po.Hitchance >= HitChance.Low && Player.Distance(po.CastPosition) < (QEmp1.Range + target.BoundingRadius))
                    {
                        castQon = po.CastPosition;
                    }
                }
                else if (Q1.IsReady() && gotManaFor(true))
                {
                    PredictionOutput po = Q1.GetPrediction(target);
                    if (po.Hitchance >= HitChance.Low && Player.Distance(po.CastPosition) < (Q1.Range + target.BoundingRadius))
                    {
                        Q1.Cast(po.CastPosition);
                    }
                }
                else if (W1.IsReady() && gotManaFor(false, true) && targetInRange(getClosestEnem(), 1000f))
                {
                    W1.Cast();
                }
                else if (!Q1.IsReady() && !W1.IsReady() && R1.IsReady() && hamQCDRem == 0 && hamECDRem == 0)// will need to add check if other form skills ready
                {
                    R1.Cast();
                }
            }
            else
            {
                if (!Q2.IsReady() && R2.IsReady() && Player.Distance(getClosestEnem()) > 350)
                {
                    R2.Cast();
                }
                if (Q2.IsReady() && gotManaFor(true) && targetInRange(target, Q2.Range))
                {
                    Q2.Cast(target);
                }
                if (E2.IsReady() && gotManaFor(false, false, true) && targetInRange(target, E2.Range) && (!gotSpeedBuff()) || (getJayceEHamDmg(target)>target.Health))
                {
                    E2.Cast(target);
                }
                if (W2.IsReady() && gotManaFor(false, true) && targetInRange(target, W2.Range))
                {
                    W2.Cast();
                }

            }
        }

        public static void doJayceInj(Obj_AI_Hero target)
        {
            if (lockedTarg != null)
                target = lockedTarg;
            else
                lockedTarg = target;


            if (isHammer)
            {


                if (inMyTowerRange(posAfterHammer(target)) && E2.IsReady())
                    E2.Cast(target);

                //If not in flash range  Q to get in it
                if (Player.Distance(target) > 400 && targetInRange(target, 600f))
                    Q2.Cast(target);

                if (!E2.IsReady() && !Q2.IsReady())
                    R2.Cast();

                if (Player.Distance(getBestPosToHammer(target)) < 400)
                {
                    Player.SummonerSpellbook.CastSpell(Player.GetSpellSlot("SummonerFlash"), getBestPosToHammer(target));
                }
                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            }
            else
            {
                if (E1.IsReady() && Q1.IsReady() && gotManaFor(true, false, true))
                {
                    PredictionOutput po = QEmp1.GetPrediction(target);
                    if (po.Hitchance >= HitChance.Low && Player.Distance(po.CastPosition) < (QEmp1.Range + target.BoundingRadius))
                    {
                        castQon = po.CastPosition;
                    }

                    // QEmp1.CastIfHitchanceEquals(target, HitChance.High);
                }
                else if (Q1.IsReady() && gotManaFor(true))
                {
                    Q1.Cast(target.Position);
                }
                else if (W1.IsReady() && gotManaFor(false, true) && targetInRange(getClosestEnem(), 1000f))
                {
                    W1.Cast();
                }
            }
        }

        public static Vector3 getBestPosToHammer(Obj_AI_Hero target )
        {
            Obj_AI_Base tower = ObjectManager.Get<Obj_AI_Turret>().Where(tur => tur.IsAlly && tur.Health > 0).OrderBy(tur => Player.Distance(tur)).First();
            return target.ServerPosition + Vector3.Normalize(tower.ServerPosition - target.ServerPosition) * (-80);
        }

        public static Vector3 posAfterHammer(Obj_AI_Base target)
        {
            return Player.ServerPosition+Vector3.Normalize(target.ServerPosition - Player.ServerPosition)*600;
        }

        public static Obj_AI_Hero getClosestEnem()
        {
            return ObjectManager.Get<Obj_AI_Hero>().Where(ene => ene.IsEnemy && ene.IsValidTarget()).OrderBy(ene => Player.Distance(ene)).First();
        }

        public static float getBestRange()
        {
            float range;
            if (!isHammer)
            {
                if (Q1.IsReady() && E1.IsReady() && gotManaFor(true, false, true))
                {
                    range = 1750;
                }
                else if (Q1.IsReady() && gotManaFor(true))
                {
                    range = 1150;
                }
                else
                {
                    range = 500;
                }
            }
            else
            {
                if (Q1.IsReady() && gotManaFor(true))
                {
                    range = 600;
                }
                else
                {
                    range = 300;
                }
            }
            return range+50;
        }


        public static bool shootQE(Vector3 pos)
        {
            if (isHammer && R2.IsReady())
                R2.Cast();
            if (!E1.IsReady() || !Q1.IsReady() || isHammer)
                return false;
            Vector3 bPos = Player.ServerPosition - Vector3.Normalize(pos - Player.ServerPosition)*50;


            Player.IssueOrder(GameObjectOrder.MoveTo, bPos);
            Q1.Cast(pos);

            E1.Cast(getParalelVec(pos));
            return true;
        }

        public static bool shouldIKnockDatMadaFaka(Obj_AI_Hero target)
        {
            if (useSmartKnock(target) && R2.IsReady() && target.CombatType == GameObjectCombatType.Melee)
            {
                return true;
            }
            float damageOn = getJayceEHamDmg(target);

            if (damageOn > target.Health * 0.9f)
            {
                return true;
            }
            if (((Player.Health / Player.MaxHealth) < 0.15f) /*&& target.CombatType == GameObjectCombatType.Melee*/)
            {
                return true;
            }
            Vector3 posAfter =target.ServerPosition + Vector3.Normalize(target.ServerPosition - Player.ServerPosition) * 450;
            if (inMyTowerRange(posAfter))
            {
                return true;
            }

            return false;
        }

        public static bool useSmartKnock(Obj_AI_Hero target)
        {
            float trueAARange = Player.BoundingRadius + target.AttackRange;
            float trueERange = target.BoundingRadius + E2.Range;

            float dist = Player.Distance(target);
            Vector2 movePos = new Vector2();
            if (target.IsMoving)
            {
                Vector2 tpos = target.Position.To2D();
                Vector2 path = target.Path[0].To2D() - tpos;
                path.Normalize();
                movePos = tpos + (path * 100);
            }
            float targ_ms = (target.IsMoving && Player.Distance(movePos) < dist) ? target.MoveSpeed : 0;
            float msDif = (Player.MoveSpeed * 0.7f - targ_ms) == 0 ? 0.0001f : (targ_ms - Player.MoveSpeed * 0.7f);
            float timeToReach = (dist - trueAARange) / msDif;
            if (dist > trueAARange && dist < trueERange && target.IsMoving)
            {
                if (timeToReach > 1.7f || timeToReach < 0.0f)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool inMyTowerRange(Vector3 pos)
        {
            return ObjectManager.Get<Obj_AI_Turret>().Where(tur => tur.IsAlly && tur.Health > 0).Any(tur => pos.Distance(tur.Position) < (850 + Player.BoundingRadius));
        }

        public static void castEonSpell(Obj_AI_Hero mis)
        {
            if (isHammer || !E1.IsReady())
                return;
            if (Player.Distance(myCastedQ.Position) < 250)
            {
                E1.Cast(getParalelVec(mis.Position));
            }

        }


        public static bool targetInRange(Obj_AI_Hero target, float range)
        {
            float dist2 = Vector2.DistanceSquared(target.ServerPosition.To2D(),Player.ServerPosition.To2D());
            float range2 = range*range + target.BoundingRadius*target.BoundingRadius;
            return dist2 < range2;
        }

        public static void checkForm()
        {
            isHammer = !Qdata.SData.Name.Contains("jayceshockblast");
        }


        public static bool gotSpeedBuff()//jaycehypercharge
        {
            return Player.Buffs.Any(bi => bi.Name.Contains("jaycehypercharge"));
        }

        public static Vector2 getParalelVec(Vector3 pos)
        {
            var v2 = Vector3.Normalize(pos - Player.ServerPosition) * 3;
            var bom = new Vector2(v2.Y, -v2.X);
            return Player.ServerPosition.To2D() + bom;
        }

        //Need to fix!!
        public static bool gotManaFor(bool q = false,bool w = false,bool e = false)
        {
            float manaNeeded = 0;
            if(q)
                manaNeeded+=Qdata.ManaCost;
            if(w)
                manaNeeded+=Wdata.ManaCost;
            if(e)
                manaNeeded+=Edata.ManaCost;
            return true;

            return manaNeeded<=Player.Mana;
        }

        public static float calcRealCD(float time)
        {
            return time + (time * Player.PercentCooldownMod);
        }

        public static void processCDs()
        {
            hamQCDRem = ((hamQCD - Game.Time)>0)?(hamQCD - Game.Time):0;
            hamWCDRem = ((hamWCD - Game.Time) > 0) ? (hamWCD - Game.Time) : 0;
            hamECDRem = ((hamECD - Game.Time) > 0) ? (hamECD - Game.Time) : 0;

            rangQCDRem = ((rangQCD - Game.Time) > 0) ? (rangQCD - Game.Time) : 0;
            rangWCDRem = ((rangWCD - Game.Time) > 0) ? (rangWCD - Game.Time) : 0;
            rangECDRem = ((rangECD - Game.Time) > 0) ? (rangECD - Game.Time) : 0;
        }

        public static void getCDs(GameObjectProcessSpellCastEventArgs spell)
        {
            if(isHammer)
            {
                if (spell.SData.Name == "JayceToTheSkies")
                    hamQCD = Game.Time + calcRealCD(hamTrueQcd[Q2.Level]);
                if (spell.SData.Name == "JayceStaticField")
                    hamWCD = Game.Time + calcRealCD(hamTrueWcd[W2.Level]);
                if (spell.SData.Name == "JayceThunderingBlow")
                    hamECD = Game.Time + calcRealCD(hamTrueEcd[E2.Level]);
            }
            else
            {
                if (spell.SData.Name == "jayceshockblast")
                    rangQCD = Game.Time + calcRealCD(rangTrueQcd[Q2.Level]);
                if (spell.SData.Name == "jaycehypercharge")
                    rangWCD = Game.Time + calcRealCD(rangTrueWcd[W2.Level]);
                if (spell.SData.Name == "jayceaccelerationgate")
                    rangECD = Game.Time + calcRealCD(rangTrueEcd[E2.Level]);
            }
        }

        public static void drawCD()
        {
            var pScreen = Drawing.WorldToScreen(Player.Position);

           // Drawing.DrawText(Drawing.WorldToScreen(Player.Position)[0], Drawing.WorldToScreen(Player.Position)[1], System.Drawing.Color.Green, "Q: wdeawd ");
            pScreen[0] -= 20;

            if (isHammer)
            {
                if (rangQCDRem == 0)
                    Drawing.DrawText(pScreen[0]-60, pScreen[1], Color.Green, "Q: Rdy");
                else
                    Drawing.DrawText(pScreen[0] - 60, pScreen[1], Color.Red, format: "Q: " + rangQCDRem.ToString("0.0"));

                if (rangWCDRem == 0)
                    Drawing.DrawText(pScreen[0], pScreen[1], Color.Green, "W: Rdy");
                else
                    Drawing.DrawText(pScreen[0], pScreen[1], Color.Red, "W: " + rangWCDRem.ToString("0.0"));

                if (rangECDRem == 0)
                    Drawing.DrawText(pScreen[0] + 60, pScreen[1], Color.Green, "E: Rdy");
                else
                    Drawing.DrawText(pScreen[0] + 60, pScreen[1], Color.Red, "E: " + rangECDRem.ToString("0.0"));
            }
            else
            {
                if (hamQCDRem == 0)
                    Drawing.DrawText(pScreen[0] - 60, pScreen[1], Color.Green, "Q: Rdy");
                else
                    Drawing.DrawText(pScreen[0] - 60, pScreen[1], Color.Red, "Q: " + hamQCDRem.ToString("0.0"));

                if (hamWCDRem == 0)
                    Drawing.DrawText(pScreen[0], pScreen[1], Color.Green, "W: Rdy");
                else
                    Drawing.DrawText(pScreen[0], pScreen[1], Color.Red, "W: " + hamWCDRem.ToString("0.0"));

                if (hamECDRem == 0)
                    Drawing.DrawText(pScreen[0] + 60, pScreen[1], Color.Green, "E: Rdy");
                else
                    Drawing.DrawText(pScreen[0] + 60, pScreen[1], Color.Red, "E: " + hamECDRem.ToString("0.0"));
            }
        }

        public static bool hammerWillKill(Obj_AI_Base target)
        {
            float damage = (float)Player.GetAutoAttackDamage(target, true)+50;
            damage += getJayceEHamDmg(target);
            damage += getJayceQHamDmg(target);

            return (target.Health < damage);
        }

        public static float getJayceEHamDmg(Obj_AI_Base target)
        {
            double percentage = 5 + (3 * Player.Spellbook.GetSpell(SpellSlot.E).Level);
            return (float)Player.CalcDamage(target, Damage.DamageType.Magical, ((target.MaxHealth / 100) * percentage) + (Player.FlatPhysicalDamageMod));
        }

        public static float getJayceQHamDmg(Obj_AI_Base target)
        {
            return (float)Player.CalcDamage(Player, Damage.DamageType.Physical, (-25 + (Player.Spellbook.GetSpell(SpellSlot.Q).Level * 45)) + (1.0 * Player.FlatPhysicalDamageMod));
        }

    }
}
