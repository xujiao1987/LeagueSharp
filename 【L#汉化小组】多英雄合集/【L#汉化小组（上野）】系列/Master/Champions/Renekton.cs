using System;
using System.Linq;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Orbwalk = MasterCommon.M_Orbwalker;

namespace Master
{
    class Renekton : Program
    {
        private Vector3 DashBackPos = default(Vector3);
        private bool ECasted = false;

        public Renekton()
        {
            SkillQ = new Spell(SpellSlot.Q, 325);
            SkillW = new Spell(SpellSlot.W, 300);
            SkillE = new Spell(SpellSlot.E, 480);
            SkillR = new Spell(SpellSlot.R, 20);
            SkillQ.SetSkillshot(SkillQ.Instance.SData.SpellCastTime, SkillQ.Instance.SData.LineWidth, SkillQ.Instance.SData.MissileSpeed, false, SkillshotType.SkillshotCircle);
            SkillE.SetSkillshot(SkillE.Instance.SData.SpellCastTime, SkillE.Instance.SData.LineWidth, SkillE.Instance.SData.MissileSpeed, false, SkillshotType.SkillshotLine);

            Config.AddSubMenu(new Menu("连招", "csettings"));
            Config.SubMenu("csettings").AddItem(new MenuItem("qusage", "使用Q").SetValue(true));
            Config.SubMenu("csettings").AddItem(new MenuItem("wusage", "使用W").SetValue(true));
            Config.SubMenu("csettings").AddItem(new MenuItem("eusage", "使用E").SetValue(true));
            Config.SubMenu("csettings").AddItem(new MenuItem("ignite", "可杀点燃").SetValue(true));
            Config.SubMenu("csettings").AddItem(new MenuItem("iusage", "使用物品").SetValue(true));

            Config.AddSubMenu(new Menu("骚扰", "hsettings"));
            Config.SubMenu("hsettings").AddItem(new MenuItem("harMode", "血量大于X骚扰").SetValue(new Slider(20, 1)));
            Config.SubMenu("hsettings").AddItem(new MenuItem("useHarQ", "使用Q").SetValue(true));
            Config.SubMenu("hsettings").AddItem(new MenuItem("useHarW", "使用W").SetValue(true));

            Config.AddSubMenu(new Menu("清线", "LaneJungClear"));
            Config.SubMenu("LaneJungClear").AddItem(new MenuItem("useClearQ", "使用Q").SetValue(true));
            Config.SubMenu("LaneJungClear").AddItem(new MenuItem("useClearW", "使用W").SetValue(true));
            Config.SubMenu("LaneJungClear").AddItem(new MenuItem("useClearE", "使用E").SetValue(true));
            Config.SubMenu("LaneJungClear").AddItem(new MenuItem("useClearI", "使用提亚马特/九头蛇").SetValue(true));

            Config.AddSubMenu(new Menu("大招", "useUlt"));
            Config.SubMenu("useUlt").AddItem(new MenuItem("surviveR", "使用R保命").SetValue(true));
            Config.SubMenu("useUlt").AddItem(new MenuItem("autouseR", "血量小于X使用R").SetValue(new Slider(20, 1)));

            Config.AddSubMenu(new Menu("杂项", "miscs"));
            Config.SubMenu("miscs").AddItem(new MenuItem("useAntiW", "使用W防突").SetValue(true));
            Config.SubMenu("miscs").AddItem(new MenuItem("useInterW", "使用W打断").SetValue(true));
            Config.SubMenu("miscs").AddItem(new MenuItem("calcelW", "取消W动画").SetValue(true));
            Config.SubMenu("miscs").AddItem(new MenuItem("CustomSkin", "换肤").SetValue(new Slider(6, 0, 6))).ValueChanged += SkinChanger;

            Config.AddSubMenu(new Menu("显示", "DrawSettings"));
            Config.SubMenu("DrawSettings").AddItem(new MenuItem("DrawQ", "Q范围").SetValue(true));
            Config.SubMenu("DrawSettings").AddItem(new MenuItem("DrawE", "E范围").SetValue(true));

            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += OnPossibleToInterrupt;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            GameObject.OnCreate += OnCreate;
            Orbwalk.AfterAttack += AfterAttack;
        }

        private void OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead || MenuGUI.IsChatOpen) return;
            switch (Orbwalk.CurrentMode)
            {
                case Orbwalk.Mode.Combo:
                    NormalCombo();
                    break;
                case Orbwalk.Mode.Harass:
                    Harass();
                    break;
                case Orbwalk.Mode.LaneClear:
                    LaneJungClear();
                    break;
                case Orbwalk.Mode.LaneFreeze:
                    LaneJungClear();
                    break;
                case Orbwalk.Mode.Flee:
                    if (SkillE.IsReady()) SkillE.Cast(Game.CursorPos, PacketCast());
                    break;
            }
        }

        private void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Config.Item("DrawQ").GetValue<bool>() && SkillQ.Level > 0) Utility.DrawCircle(Player.Position, SkillQ.Range, SkillQ.IsReady() ? Color.Green : Color.Red);
            if (Config.Item("DrawE").GetValue<bool>() && SkillE.Level > 0) Utility.DrawCircle(Player.Position, SkillE.Range, SkillE.IsReady() ? Color.Green : Color.Red);
        }

        private void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Config.Item("useAntiW").GetValue<bool>()) return;
            if (gapcloser.Sender.IsValidTarget(SkillE.Range) && (SkillW.IsReady() || Player.HasBuff("RenektonPreExecute", true)))
            {
                if (!Player.HasBuff("RenektonPreExecute", true)) SkillW.Cast(PacketCast());
                if (Player.HasBuff("RenektonPreExecute", true)) Player.IssueOrder(GameObjectOrder.AttackUnit, gapcloser.Sender);
            }
        }

        private void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Config.Item("useInterW").GetValue<bool>()) return;
            if (SkillW.IsReady() && SkillE.IsReady() && !SkillW.InRange(unit.Position) && unit.IsValidTarget(SkillE.Range)) SkillE.Cast(unit.Position + Vector3.Normalize(unit.Position - Player.Position) * 200, PacketCast());
            if (unit.IsValidTarget(SkillW.Range) && (SkillW.IsReady() || Player.HasBuff("RenektonPreExecute", true)))
            {
                if (!Player.HasBuff("RenektonPreExecute", true)) SkillW.Cast(PacketCast());
                if (Player.HasBuff("RenektonPreExecute", true)) Player.IssueOrder(GameObjectOrder.AttackUnit, unit);
            }
        }

        private void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;
            if (args.SData.Name == "RenektonSliceAndDice")
            {
                ECasted = true;
                Utility.DelayAction.Add(400, () => ECasted = false);
                if (Orbwalk.CurrentMode == Orbwalk.Mode.Harass && DashBackPos == default(Vector3) && ECasted) DashBackPos = Player.Position + (Player.Position - targetObj.Position) * SkillE.Range;
            }
        }

        private void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender is Obj_SpellMissile && sender.IsValid && Config.Item("surviveR").GetValue<bool>() && SkillR.IsReady())
            {
                var missle = (Obj_SpellMissile)sender;
                var caster = missle.SpellCaster;
                if (caster.IsEnemy)
                {
                    if (missle.SData.Name.Contains("BasicAttack"))
                    {
                        if (missle.Target.IsMe && (Player.Health - caster.GetAutoAttackDamage(Player, true)) * 100 / Player.MaxHealth <= Config.Item("autouseR").GetValue<Slider>().Value) SkillR.Cast();
                    }
                    else if (missle.Target.IsMe || missle.EndPosition.Distance(Player.Position) <= 130)
                    {
                        if (missle.SData.Name == "summonerdot")
                        {
                            if ((Player.Health - (caster as Obj_AI_Hero).GetSummonerSpellDamage(Player, Damage.SummonerSpell.Ignite)) * 100 / Player.MaxHealth <= Config.Item("autouseR").GetValue<Slider>().Value) SkillR.Cast();
                        }
                        else if ((Player.Health - (caster as Obj_AI_Hero).GetSpellDamage(Player, (caster as Obj_AI_Hero).GetSpellSlot(missle.SData.Name, false), 1)) * 100 / Player.MaxHealth <= Config.Item("autouseR").GetValue<Slider>().Value) SkillR.Cast();
                    }
                }
            }
        }

        private void AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (!unit.IsMe || Orbwalk.CurrentMode != Orbwalk.Mode.Combo || Orbwalk.CurrentMode != Orbwalk.Mode.Harass) return;
            if (Config.Item("calcelW").GetValue<bool>() && target.HasBuffOfType(BuffType.Stun) && target.Buffs.FirstOrDefault(i => i.SourceName == Name) != null && target is Obj_AI_Hero && target.IsValidTarget(350))
            {
                if (Items.CanUseItem(Tiamat)) Items.UseItem(Tiamat);
                if (Items.CanUseItem(Hydra)) Items.UseItem(Hydra);
            }
        }

        private void NormalCombo()
        {
            if (targetObj == null) return;
            if (Config.Item("eusage").GetValue<bool>() && SkillE.IsReady() && SkillE.Instance.Name == "RenektonSliceAndDice" && SkillE.InRange(targetObj.Position)) SkillE.Cast(targetObj.Position + Vector3.Normalize(targetObj.Position - Player.Position) * 200, PacketCast());
            if (Config.Item("wusage").GetValue<bool>() && SkillW.InRange(targetObj.Position) && !ECasted && SkillW.IsReady()) SkillW.Cast(PacketCast());
            if (Config.Item("qusage").GetValue<bool>() && SkillQ.IsReady() && SkillQ.InRange(targetObj.Position) && !ECasted) SkillQ.Cast(PacketCast());
            if (Config.Item("eusage").GetValue<bool>() && SkillE.IsReady() && SkillE.Instance.Name != "RenektonSliceAndDice" && !ECasted && SkillE.InRange(targetObj.Position)) SkillE.Cast(targetObj.Position + Vector3.Normalize(targetObj.Position - Player.Position) * 200, PacketCast());
            if (Config.Item("iusage").GetValue<bool>() && !ECasted) UseItem(targetObj);
            if (Config.Item("ignite").GetValue<bool>()) CastIgnite(targetObj);
        }

        private void Harass()
        {
            if (targetObj == null) return;
            var HpEnough = Player.Health * 100 / Player.MaxHealth >= Config.Item("harMode").GetValue<Slider>().Value;
            if (SkillE.IsReady() && SkillE.Instance.Name == "RenektonSliceAndDice" && SkillE.InRange(targetObj.Position) && HpEnough) SkillE.Cast(targetObj.Position + Vector3.Normalize(targetObj.Position - Player.Position) * 200, PacketCast());
            if (Config.Item("useHarW").GetValue<bool>() && SkillW.InRange(targetObj.Position) && !ECasted && SkillW.IsReady()) SkillW.Cast(PacketCast());
            if (Config.Item("useHarQ").GetValue<bool>() && SkillQ.IsReady() && SkillQ.InRange(targetObj.Position) && !ECasted) SkillQ.Cast(PacketCast());
            if (SkillE.IsReady() && SkillE.Instance.Name != "RenektonSliceAndDice" && !ECasted && DashBackPos != default(Vector3)) SkillE.Cast(DashBackPos, PacketCast());
            if (!SkillE.IsReady()) DashBackPos = default(Vector3);
        }

        private void LaneJungClear()
        {
            var minionObj = MinionManager.GetMinions(Player.Position, SkillE.Range - 50, MinionTypes.All, MinionTeam.NotAlly);
            if (minionObj.Count == 0) return;
            var posEFarm = SkillE.GetLineFarmLocation(minionObj);
            if (Config.Item("useClearE").GetValue<bool>() && SkillE.IsReady() && SkillE.Instance.Name == "RenektonSliceAndDice") SkillE.Cast(posEFarm.MinionsHit >= 2 ? posEFarm.Position : minionObj.First().Position.To2D(), PacketCast());
            if (Config.Item("useClearQ").GetValue<bool>() && SkillQ.IsReady() && minionObj.Count(i => i.IsValidTarget(SkillQ.Range)) >= 2 && !ECasted) SkillQ.Cast(PacketCast());
            if (Config.Item("useClearW").GetValue<bool>() && !ECasted && SkillW.IsReady() && minionObj.FirstOrDefault(i => SkillW.InRange(i.Position) && (Player.Mana >= SkillW.Instance.ManaCost) ? CanKill(i, SkillW, 1) : CanKill(i, SkillW)) != null) SkillW.Cast(PacketCast());
            if (Config.Item("useClearE").GetValue<bool>() && SkillE.IsReady() && SkillE.Instance.Name != "RenektonSliceAndDice" && !ECasted) SkillE.Cast(posEFarm.MinionsHit >= 2 ? posEFarm.Position : minionObj.First().Position.To2D(), PacketCast());
            if (Config.Item("useClearI").GetValue<bool>() && minionObj.Count(i => i.IsValidTarget(350)) >= 2 && !ECasted)
            {
                if (Items.CanUseItem(Tiamat)) Items.UseItem(Tiamat);
                if (Items.CanUseItem(Hydra)) Items.UseItem(Hydra);
            }
        }

        private void UseItem(Obj_AI_Hero target)
        {
            if (!Config.Item("calcelW").GetValue<bool>() || (Config.Item("calcelW").GetValue<bool>() && !Player.HasBuff("RenektonPreExecute", true)))
            {
                if (Items.CanUseItem(Tiamat) && Player.CountEnemysInRange(350) >= 1) Items.UseItem(Tiamat);
                if (Items.CanUseItem(Hydra) && (Player.CountEnemysInRange(350) >= 2 || (Player.GetAutoAttackDamage(target) < target.Health && Player.CountEnemysInRange(350) == 1))) Items.UseItem(Hydra);
            }
            if (Items.CanUseItem(Rand) && Player.CountEnemysInRange(450) >= 1) Items.UseItem(Rand);
        }
    }
}