#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace FedCaitlyn
{
    internal class Program
    {
        public const string ChampionName = "Caitlyn";
        public static Orbwalking.Orbwalker Orbwalker;

        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q, W, E, R;
        public static Vector2 PingLocation;
        public static int LastPingT = 0;
        public static int EQComboT = 0;

        public static Menu Config;
        private static Obj_AI_Hero Player;

        const float _spellQSpeed = 2500;
        const float _spellQSpeedMin = 400;

        public static Geometrys.Rectangle rect;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            if (Player.BaseSkinName != ChampionName) return;

            Q = new Spell(SpellSlot.Q, 1300);
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 950);
            R = new Spell(SpellSlot.R, 2000);

            Q.SetSkillshot(0.5f, 90f, 2200f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.25f, 80f, 2000f, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 80f, 1600f, true, SkillshotType.SkillshotLine);

            SpellList.AddRange(new[] { Q, W, E, R });

            Config = new Menu("|鍒濊姹夊寲-濂宠|" + ChampionName, ChampionName, true);

            var targetSelectorMenu = new Menu("|鍒濊姹夊寲-鐩爣閫夋嫨|", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Config.AddSubMenu(new Menu("|鍒濊姹夊寲-璧扮爫|", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("|鍒濊姹夊寲-Q璁剧疆|", "Piltover"));
            Config.SubMenu("Piltover").AddItem(new MenuItem("QMin", "Q鍙湁AA鐨勮寖鍥淬€€").SetValue(true));
            Config.SubMenu("Piltover").AddItem(new MenuItem("UseQ", "浣跨敤Q妯″紡: ").SetValue(new StringList(new[] { "杩炴嫑", "楠氭壈", "涓や釜", "No" }, 1)));
            Config.SubMenu("Piltover").AddItem(new MenuItem("KillQ", "鑷姩Q鍑绘潃").SetValue(true));            
            Config.SubMenu("Piltover").AddItem(new MenuItem("autoccQ", "鑷姩Q琚帶鍒剁殑鏁屼汉").SetValue(true));
            Config.SubMenu("Piltover").AddItem(new MenuItem("autoQMT", "鑷姩Q澶氫釜鐩爣").SetValue(true));
            Config.SubMenu("Piltover").AddItem(new MenuItem("minAutoQMT", "澶氬皯鐩爣").SetValue(new Slider(3, 2, 5)));
            Config.SubMenu("Piltover").AddItem(new MenuItem("UseQFarm", "鐢≦娓呯嚎").SetValue(true));
            Config.SubMenu("Piltover").AddItem(new MenuItem("minMana", "娓呯嚎/鍐滃満娉曞姏 %").SetValue(new Slider(40, 100, 0)));

            Config.AddSubMenu(new Menu("|鍒濊姹夊寲-闄烽槺璁剧疆|", "Trap"));
            Config.SubMenu("Trap").AddItem(new MenuItem("autoccW", "琚帶鍒剁殑鑻遍泟鑷姩W闄烽槺").SetValue(true));
            Config.SubMenu("Trap").AddItem(new MenuItem("autotpW", "浣跨敤W闄烽槺鍦═P").SetValue(true));
            Config.SubMenu("Trap").AddItem(new MenuItem("autoRevW", "鑷姩W闄烽槺鍦ㄦ槬鍝ョ敳").SetValue(true));
            Config.SubMenu("Trap").AddItem(new MenuItem("AGCtrap", "闃叉绐佽繘W").SetValue(true));
            Config.SubMenu("Trap").AddItem(new MenuItem("casttrap", "闄烽槺瀵规渶杩戠殑鏁屼汉 - 寰呭姙浜嬮」鏃犳晥").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("|鍒濊姹夊寲-E鎶€鑳借缃畖", "90 Caliber"));
            Config.SubMenu("90 Caliber").AddItem(new MenuItem("AGConoff", "琚帶鍒剁殑鑻遍泟鑷姩W闄烽槺").SetValue(true));
            Config.SubMenu("90 Caliber").AddItem(new MenuItem("KillEQ", "鑷姩E-Q鍑绘潃銆€").SetValue(true));
            Config.SubMenu("90 Caliber").AddItem(new MenuItem("UseEQC", "浣跨敤E-Q杩炴嫑").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("90 Caliber").AddItem(new MenuItem("PeelE", "浣跨敤E闃插畧").SetValue(true));
            Config.SubMenu("90 Caliber").AddItem(new MenuItem("JumpE", "浣跨敤E鍒伴紶鏍囨柟鍚戙€€").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("|鍒濊姹夊寲-R璁剧疆|", "Ace Hole"));
            Config.SubMenu("Ace Hole").AddItem(new MenuItem("rKill", "鎵嬪姩R鍑绘潃").SetValue(new KeyBind("R".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Ace Hole").AddItem(new MenuItem("AutoRKill", "鑷姩浣跨敤R鍑绘潃").SetValue(true));
            Config.SubMenu("Ace Hole").AddItem(new MenuItem("pingkillable", "ping 鍙嚮鏉€鑻遍泟").SetValue(true));

            Config.AddSubMenu(new Menu("|鍒濊姹夊寲-鎶€鑳借寖鍥撮€夐」|", "Drawing"));
            Config.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "绂佺敤鎵€鏈夈€€").SetValue(false));
            Config.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "鑼冨洿 Q").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.SubMenu("Drawing").AddItem(new MenuItem("Draw_W", "鑼冨洿 W").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.SubMenu("Drawing").AddItem(new MenuItem("Draw_E", "鑼冨洿 E").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.SubMenu("Drawing").AddItem(new MenuItem("Draw_R", "鑼冨洿 R").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.SubMenu("Drawing").AddItem(new MenuItem("DrawRRangeM", "鐢籖鑼冨洿 (銆€灏忓湴鍥俱€€)").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.AddItem(new MenuItem("UsePacket","浣跨敤灏佸寘").SetValue(true));
            Config.AddSubMenu(new Menu("|鍒濊姹夊寲-缇ゅ彿|", "by chujian"));
            Config.SubMenu("by chujian").AddItem(new MenuItem("qunhao", "姹夊寲缇わ細386289593"));
            Config.SubMenu("by chujian").AddItem(new MenuItem("qunhao1", "浜ゆ祦缇わ細333399"));
			Config.AddToMainMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            GameObject.OnCreate += Trap_OnCreate;

            Game.PrintChat("<font color=\"#00BFFF\">鍒濊姹夊寲" + ChampionName + " -</font> <font color=\"#FFFFFF\">鍔犺浇鎴愬姛!QQ藟5011477 !</font>");

        }

        private static void Game_OnGameUpdate(EventArgs args)
        { 
            if (ObjectManager.Player.IsDead) return;

            var Qmode = Config.Item("UseQ").GetValue<StringList>().SelectedIndex;

            switch (Program.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (Qmode == 0 || Qmode == 2)
                        Cast_BasicLineSkillshot_Enemy(Q);
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if ((Qmode == 1 || Qmode == 2) && GetManaPercent() >= Config.Item("minMana").GetValue<Slider>().Value)
                        Cast_BasicLineSkillshot_Enemy(Q);
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    if (Config.Item("UseQFarm").GetValue<bool>() && GetManaPercent() >= Config.Item("minMana").GetValue<Slider>().Value)
                        Cast_BasicLineSkillshot_AOE_Farm(Q);
                    break;
            }
            
            if (Config.Item("rKill").GetValue<KeyBind>().Active || Config.Item("AutoRKill").GetValue<bool>())
            {
                AutoRKill();
            }

            if (Config.Item("autoccW").GetValue<bool>() || Config.Item("autoccQ").GetValue<bool>())
            {
                AutoCC();
            }            

            if (Config.Item("PeelE").GetValue<bool>())
            {
                PeelE();
            }

            if (Config.Item("JumpE").GetValue<KeyBind>().Active)
            {
                JumptoMouse();
            }

            if (Config.Item("UseEQC").GetValue<KeyBind>().Active)
            {
                ComboEQ();
            }

            if (Config.Item("KillQ").GetValue<bool>() || Config.Item("KillEQ").GetValue<bool>())
            {
                Killer();
            }

            if (Config.Item("autoQMT").GetValue<bool>())
            {
                AutoQMT();
            }

            if (R.IsReady() && Config.Item("pingkillable").GetValue<bool>())
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(GetRRange()) && (float)ObjectManager.Player.GetSpellDamage(h, SpellSlot.R) * 0.9 > h.Health))
                {
                    Ping(enemy.Position.To2D());
                }
            }
        }

        private static float GetRRange()
        {
            return 1500 + (500 * R.Level);
        }

        private static void PeelE()
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsValidTarget(E.Range) && enemy.Distance(ObjectManager.Player) <= enemy.BoundingRadius + enemy.AttackRange + ObjectManager.Player.BoundingRadius && enemy.IsMelee())
                {
                    E.Cast(enemy);
                }
            }
        }

        private static void JumptoMouse()
        {
            if (E.IsReady())
            {
                var pos = ObjectManager.Player.ServerPosition.To2D().Extend(Game.CursorPos.To2D(), -300).To3D();
                E.Cast(pos, true);
            }
        }   

        private static void ComboEQ()
        {
            var vTarget = SimpleTs.GetTarget(E.Range - 30, SimpleTs.DamageType.Physical);

            if (vTarget.IsValidTarget(E.Range) && E.IsReady() && Q.IsReady())
            {
                var prediction = E.GetPrediction(vTarget);
                if (prediction.Hitchance >= HitChance.High)
                {
                    E.Cast(vTarget, true);
                    EQComboT = Environment.TickCount;
                }
            }
        }        

        private static float GetDynamicQSpeed(float distance)
        {
            float accelerationrate = Q.Range / (_spellQSpeedMin - _spellQSpeed); // = -0.476...
            return _spellQSpeed + accelerationrate * distance;
        }

        private static void Killer()
        {
            var qTarget = SimpleTs.GetTarget(Q.Range - 50, SimpleTs.DamageType.Physical);
            var eTarget = SimpleTs.GetTarget(E.Range - 50, SimpleTs.DamageType.Physical);
            var QonlyAA = Config.Item("QMin").GetValue<bool>();

            if (QonlyAA && Orbwalking.InAutoAttackRange(qTarget)) return;

            if (Config.Item("KillQ").GetValue<bool>() && Config.Item("KillEQ").GetValue<bool>() && Q.IsReady() && E.IsReady() && eTarget.Health < (ObjectManager.Player.GetSpellDamage(eTarget, SpellSlot.E) + ObjectManager.Player.GetSpellDamage(eTarget, SpellSlot.Q)) * 0.9)
            {
                PredictionOutput ePred = E.GetPrediction(eTarget);
                if (ePred.Hitchance >= HitChance.High)
                    E.Cast(eTarget, true, true);

                Vector3 predictedPos = Prediction.GetPrediction(qTarget, Q.Delay).UnitPosition;
                Q.Speed = GetDynamicQSpeed(ObjectManager.Player.Distance(predictedPos));
                Q.CastIfHitchanceEquals(qTarget, HitChance.High, true);

            }
            else
            {
                if (Config.Item("KillQ").GetValue<bool>() && Q.IsReady() && qTarget.Health < (ObjectManager.Player.GetSpellDamage(qTarget, SpellSlot.Q) * 0.9))
                {
                    Vector3 predictedPos = Prediction.GetPrediction(qTarget, Q.Delay).UnitPosition;
                    Q.Speed = GetDynamicQSpeed(ObjectManager.Player.Distance(predictedPos));
                    Q.CastIfHitchanceEquals(qTarget, HitChance.High, true);
                }
                else
                {
                    if (Config.Item("KillEQ").GetValue<bool>() && !Q.IsReady() && E.IsReady() && eTarget.Health < (ObjectManager.Player.GetSpellDamage(qTarget, SpellSlot.Q) * 0.9))
                    {
                        Vector3 predictedPos = Prediction.GetPrediction(qTarget, Q.Delay).UnitPosition;
                        Q.Speed = GetDynamicQSpeed(ObjectManager.Player.Distance(predictedPos));
                        Q.CastIfHitchanceEquals(qTarget, HitChance.High, true);
                    }
                }
            }
        }

        private static void AutoRKill()
        {
            var rTarget = SimpleTs.GetTarget(GetRRange() - 100, SimpleTs.DamageType.Physical);

            if (R.IsReady() && rTarget != null && rTarget.Health < ObjectManager.Player.GetSpellDamage(rTarget, SpellSlot.R) * 0.9)
            {
                if (ObjectManager.Player.Distance(rTarget) > Q.Range)
                    R.CastOnUnit(rTarget);
            }
        }

        private static void AutoCC()
        {
            List<Obj_AI_Hero> enemBuffed = getEnemiesBuffs();
            foreach (Obj_AI_Hero enem in enemBuffed)
            {                 
                if (W.IsReady() && Config.Item("autoccW").GetValue<bool>())
                {
                    W.CastOnUnit(enem);
                }

                if (Q.IsReady() && Config.Item("autoccQ").GetValue<bool>())
                {
                    if (Q.GetPrediction(enem).Hitchance >= HitChance.High)
                        Q.Cast(enem, true);
                }
            }
        }

        public static List<Obj_AI_Hero> getEnemiesBuffs()
        {
            List<Obj_AI_Hero> enemBuffs = new List<Obj_AI_Hero>();
            foreach (Obj_AI_Hero enem in ObjectManager.Get<Obj_AI_Hero>().Where(enem => enem.IsEnemy))
            {
                foreach (BuffInstance buff in enem.Buffs)
                {
                    if (buff.Name == "zhonyasringshield" || buff.Name == "caitlynyordletrapdebuff" || buff.Name == "powerfistslow" || buff.Name == "aatroxqknockup" || buff.Name == "ahriseducedoom" ||
                        buff.Name == "CurseoftheSadMummy" || buff.Name == "braumstundebuff" || buff.Name == "braumpulselineknockup" || buff.Name == "rupturetarget" || buff.Name == "EliseHumanE" ||
                        buff.Name == "HowlingGaleSpell" || buff.Name == "jarvanivdragonstrikeph2" || buff.Name == "karmaspiritbindroot" || buff.Name == "LuxLightBindingMis" || buff.Name == "lissandrawfrozen" ||
                        buff.Name == "lissandraenemy2" || buff.Name == "unstoppableforceestun" || buff.Name == "maokaiunstablegrowthroot" || buff.Name == "monkeykingspinknockup" || buff.Name == "DarkBindingMissile" ||
                        buff.Name == "namiqdebuff" || buff.Name == "nautilusanchordragroot" || buff.Name == "RunePrison" || buff.Name == "SonaR" || buff.Name == "sejuaniglacialprison" || buff.Name == "swainshadowgrasproot" ||
                        buff.Name == "threshqfakeknockup" || buff.Name == "VeigarStun" || buff.Name == "velkozestun" || buff.Name == "virdunkstun" || buff.Name == "viktorgravitonfieldstun" || buff.Name == "yasuoq3mis" ||
                        buff.Name == "zyragraspingrootshold" || buff.Name == "zyrabramblezoneknockup" || buff.Name == "katarinarsound" || buff.Name == "lissandrarself" || buff.Name == "AlZaharNetherGrasp" || buff.Name == "Meditate" ||
                        buff.Name == "missfortunebulletsound" || buff.Name == "AbsoluteZero" || buff.Name == "pantheonesound" || buff.Name == "VelkozR" || buff.Name == "infiniteduresssound" || buff.Name == "chronorevive" ||
                        buff.Type == BuffType.Suppression || buff.Name == "aatroxpassivedeath" || buff.Name == "zacrebirthstart")
                    {
                        enemBuffs.Add(enem);
                        break;
                    }
                }
            }
            return enemBuffs;
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (Config.Item("Draw_Disabled").GetValue<bool>())
                return;

            if (R.Level == 0) return;
            var menuItem = Config.Item("DrawRRangeM").GetValue<Circle>();
            if (menuItem.Active)
                Utility.DrawCircle(ObjectManager.Player.Position, GetRRange(), menuItem.Color, 2, 30, true);

        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("Draw_Disabled").GetValue<bool>())
                return;

            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item("Draw_" + spell.Slot).GetValue<Circle>();
                if (menuItem.Active && (spell.Slot != SpellSlot.R || R.Level > 0))
                    Utility.DrawCircle(ObjectManager.Player.Position, spell.Range, spell.IsReady() ? menuItem.Color : Color.Red);
            }          

        }

        private static void Ping(Vector2 position)
        {
            if (Environment.TickCount - LastPingT < 30 * 1000) return;
            LastPingT = Environment.TickCount;
            PingLocation = position;
            SimplePing();
            Utility.DelayAction.Add(150, SimplePing);
            Utility.DelayAction.Add(300, SimplePing);
            Utility.DelayAction.Add(400, SimplePing);
            Utility.DelayAction.Add(800, SimplePing);
        }

        private static void SimplePing()
        {
            Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(PingLocation.X, PingLocation.Y, 0, 0, Packet.PingType.Fallback)).Process();
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (E.IsReady() && gapcloser.Sender.IsValidTarget(E.Range))
                E.Cast(gapcloser.Sender);
        }

        private static void Trap_OnCreate(LeagueSharp.GameObject Trap, EventArgs args)
        {
            if (ObjectManager.Player.Spellbook.CanUseSpell(SpellSlot.W) != SpellState.Ready || 
                (!Config.Item("autotpW").GetValue<bool>() && !Config.Item("autoRevW").GetValue<bool>()))
                return;   

                // Teleport
                if (Config.Item("autotpW").GetValue<bool>())
                {
                    if (Trap.Name.Contains("GateMarker_red") || Trap.Name == "Pantheon_Base_R_indicator_red.troy" || Trap.Name.Contains("teleport_target_red") ||
                        Trap.Name == "LeBlanc_Displacement_Yellow_mis.troy" || Trap.Name == "Leblanc_displacement_blink_indicator_ult.troy" || Trap.Name.Contains("Crowstorm"))
                    {
                        if (Trap.IsEnemy)
                        {

                            var target = ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(enemy => enemy.IsEnemy && enemy.Distance(Trap.Position) < W.Range);
                            ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W, target);

                        }
                    }
                }
              
                // Revive
                if (Config.Item("autoRevW").GetValue<bool>())
                {
                    if (Trap.Name == "LifeAura.troy")
                    {
                        if (Trap.IsEnemy)
                        {

                            var target = ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(enemy => enemy.IsEnemy && enemy.Distance(Trap.Position) < W.Range);
                            ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W, target);

                        }
                    }
                }
            }         

        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && Environment.TickCount - EQComboT < 500 &&
                (args.SData.Name.Contains("CaitlynEntrapment")))
            {
                Q.Cast(args.End, true);
            }
        }

        private static Obj_AI_Hero GetEnemyHitByQ(Spell Q, int numHit)
        {
            int totalHit = 0;
            Obj_AI_Hero target = null;

            foreach (Obj_AI_Hero current in ObjectManager.Get<Obj_AI_Hero>())
            {

                var prediction = Q.GetPrediction(current, true);

                if (Vector3.Distance(ObjectManager.Player.Position, prediction.CastPosition) <= Q.Range - 50)
                {

                    Vector2 extended = current.Position.To2D().Extend(ObjectManager.Player.Position.To2D(), -Q.Range + Vector2.Distance(ObjectManager.Player.Position.To2D(), current.Position.To2D()));
                    rect = new Geometrys.Rectangle(ObjectManager.Player.Position.To2D(), extended, Q.Width);

                    if (!current.IsMe && current.IsEnemy)
                    {                        
                        totalHit = 1;
                        foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
                        {
                            if (enemy.IsEnemy && current.ChampionName != enemy.ChampionName && !enemy.IsDead && !rect.ToPolygon().IsOutside(enemy.Position.To2D()))
                            {
                                totalHit += 1;
                            }
                        }
                    }

                    if (totalHit >= numHit)
                    {
                        target = current;
                        break;
                    }
                }

            }
            
            return target;
        }

        private static void AutoQMT()
        {
            var minHit = GetEnemyHitByQ(Q, Config.Item("minAutoQMT").GetValue<Slider>().Value);

            if (minHit != null)
            {
                var QonlyAA = Config.Item("QMin").GetValue<bool>();
                if (QonlyAA && Orbwalking.InAutoAttackRange(minHit)) return;

                Q.Cast(minHit, true);
            }
        }

        private static Obj_AI_Hero Cast_BasicLineSkillshot_Enemy(Spell spell, SimpleTs.DamageType damageType = SimpleTs.DamageType.Physical)
        {
            var QonlyAA = Config.Item("QMin").GetValue<bool>();
            var target = SimpleTs.GetTarget(spell.Range, damageType);

            if (!spell.IsReady())
                return null;
            
            if (target == null)
                return null;

            if (QonlyAA && Orbwalking.InAutoAttackRange(target)) return null;

            if (!target.IsValidTarget(spell.Range) || spell.GetPrediction(target).Hitchance < HitChance.High)
                return null;

            spell.Cast(target, true);
            return target;
        }

        private static void Cast_BasicLineSkillshot_AOE_Farm(Spell spell)
        {
            if (!spell.IsReady()) return;

            var minions = MinionManager.GetMinions(ObjectManager.Player.Position, spell.Range, MinionTypes.All, MinionTeam.NotAlly);

            if (minions.Count == 0) return;

            var castPostion = MinionManager.GetBestLineFarmLocation(minions.Select(minion => minion.ServerPosition.To2D()).ToList(), spell.Width - 10, spell.Range);

            spell.Cast(castPostion.Position, true);
        }

        private static float GetManaPercent()
        {
            return (ObjectManager.Player.Mana / ObjectManager.Player.MaxMana) * 100f;
        }
    }
}
