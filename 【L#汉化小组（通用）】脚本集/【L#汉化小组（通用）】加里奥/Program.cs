using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.CompilerServices;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SKO_Galio
{
    class Program
    {
        private const string ChampionName = "Galio";
        private static Menu Config;
        private static Orbwalking.Orbwalker Orbwalker;
        private static List<Spell> SpellList = new List<Spell>();
        private static Spell Q, W, E, R;
        private static Items.Item DFG, HDR, BKR, BWC, YOU;
        private static Obj_AI_Hero Player;
        private static SpellSlot IgniteSlot;
        private static bool PacketCast;


        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad; 
        }

        private static void OnGameLoad(EventArgs args) 
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;

            Q = new Spell(SpellSlot.Q, 940f);
            W = new Spell(SpellSlot.W, 700f);
            E = new Spell(SpellSlot.E, 1180f);
            R = new Spell(SpellSlot.R, 560f);

            Q.SetSkillshot(0.5f, 120, 1300, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.5f, 140, 1200, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.5f, 300, 0, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            HDR = new Items.Item(3074, Player.AttackRange+50);
            BKR = new Items.Item(3153, 450f);
            BWC = new Items.Item(3144, 450f);
            YOU = new Items.Item(3142, 185f);
            DFG = new Items.Item(3128, 750f);

            IgniteSlot = Player.GetSpellSlot("SummonerDot");

            //TargetSelector
            var targetSelectorMenu = new Menu("目标选择", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            Config.AddSubMenu(new Menu("走砍", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Combo
            Config.AddSubMenu(new Menu("连招", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "使用Q")).SetValue(true);
			Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "使用W")).SetValue(true);
			Config.SubMenu("Combo").AddItem(new MenuItem("WMode", "W模式")).SetValue<StringList>(new StringList(new[] {"一直", "大招"}, 1));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "使用E")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "使用R")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("MinEnemys", "敌人大于X使用R")).SetValue(new Slider(3, 5, 1));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseItems", "使用物品")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo", "热键").SetValue(new KeyBind(Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));

            //Extra
            Config.AddSubMenu(new Menu("其他", "Extra"));
            Config.SubMenu("Extra").AddItem(new MenuItem("AutoShield", "自动盾")).SetValue(false);
            Config.SubMenu("Extra").AddItem(new MenuItem("UsePacket", "使用封包").SetValue(true));


            //Harass
            Config.AddSubMenu(new Menu("骚扰", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "使用Q")).SetValue(true);
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "使用E")).SetValue(true);
            Config.SubMenu("Harass").AddItem(new MenuItem("ActiveHarass", "热键").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));

            //Farm
            Config.AddSubMenu(new Menu("清线", "Lane"));
            Config.SubMenu("Lane").AddItem(new MenuItem("UseQLane", "使用Q")).SetValue(true);
            Config.SubMenu("Lane").AddItem(new MenuItem("UseELane", "使用E")).SetValue(true);
            Config.SubMenu("Lane").AddItem(new MenuItem("ActiveLane", "热键").SetValue(new KeyBind(Config.Item("LaneClear").GetValue<KeyBind>().Key, KeyBindType.Press)));

            //Kill Steal
            Config.AddSubMenu(new Menu("抢头", "Ks"));
            Config.SubMenu("Ks").AddItem(new MenuItem("ActiveKs", "抢头")).SetValue(true);
            Config.SubMenu("Ks").AddItem(new MenuItem("UseQKs", "使用Q")).SetValue(true);
            Config.SubMenu("Ks").AddItem(new MenuItem("UseEKs", "使用E")).SetValue(true);
            Config.SubMenu("Ks").AddItem(new MenuItem("UseIgnite", "使用点燃")).SetValue(true);


            //Drawings
            Config.AddSubMenu(new Menu("显示", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Q范围")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "E范围")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "R范围")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "无延迟线圈").SetValue(true));
            Config.SubMenu("Drawings").AddItem(new MenuItem("CircleQuality", "质量").SetValue(new Slider(100, 100, 10)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("CircleThickness", "密度").SetValue(new Slider(1, 10, 1)));

            Config.AddToMainMenu();

            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;

            Game.PrintChat("SKO Galio Loaded!");

        }

        private static void OnGameUpdate(EventArgs args)
        {
			PacketCast = Config.Item("UsePacket").GetValue<bool>();

            Orbwalker.SetAttack(true);

			var allminions = MinionManager.GetMinions(Player.ServerPosition, 1000, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);

			if(Config.Item("ActiveLane").GetValue<KeyBind>().Active)
			{
				foreach(var m in allminions)
				{
					if(m.IsValidTarget())
					{
						if(Q.IsReady() && Config.Item("UseQLane").GetValue<bool>() && Player.Distance(m) <= Q.Range)
						{
							Q.CastOnUnit(m, PacketCast);
						}
						if(E.IsReady() && Config.Item("UseELane").GetValue<bool>() && Player.Distance(m) <= E.Range)
						{
							E.Cast(m, PacketCast);
						}
					}
				}
			}

			var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);

            if (Config.Item("ActiveCombo").GetValue<KeyBind>().Active) {
				Combo(target);
            }
            if (Config.Item("ActiveHarass").GetValue<KeyBind>().Active)
            {
				Harass(target);
            }
            if (Config.Item("ActiveKs").GetValue<bool>())
            {
				KillSteal(target);
            }
        }

		private static void Combo(Obj_AI_Hero target) {
            if (!Player.HasBuff("GalioIdolOfDurand")) {
                Orbwalker.SetMovement(true);
            }

            if (target.IsValidTarget()) 
            {
                if (Q.IsReady() && Player.Distance(target) <= Q.Range && Config.Item("UseQCombo").GetValue<bool>())
                {
                    Q.Cast(target, PacketCast);
                }
                if (E.IsReady() && Player.Distance(target) <= E.Range && Config.Item("UseECombo").GetValue<bool>())
                {
                    E.Cast(target, PacketCast);
                } 
				if (Config.Item("UseWCombo").GetValue<bool>() && Config.Item("WMode").GetValue<StringList>().SelectedIndex == 0 && W.IsReady())
				{
					W.Cast(Player);
				}
					
				if (R.IsReady() && GetEnemys(target) >= Config.Item("MinEnemys").GetValue<Slider>().Value && Config.Item("UseRCombo").GetValue<bool>())
                {
                    Orbwalker.SetMovement(false);
                    R.Cast(target, PacketCast, true);
					if (Config.Item("UseWCombo").GetValue<bool>() && Config.Item("WMode").GetValue<StringList>().SelectedIndex == 1 && W.IsReady())
					{
						W.Cast(Player);
					}
                }
            
            }
        }

		private static void Harass(Obj_AI_Hero target){
            if (target.IsValidTarget()){
                if (Q.IsReady() && Player.Distance(target) <= Q.Range && Config.Item("UseQHarass").GetValue<bool>())
                {
                    Q.Cast(target, PacketCast);
                }
                else if (E.IsReady() && Player.Distance(target) <= E.Range && Config.Item("UseEHarass").GetValue<bool>())
                {
                    E.Cast(target, PacketCast);
                }
            }
        }
			

		private static void KillSteal(Obj_AI_Hero target) {
            var IgniteDmg = Damage.GetSummonerSpellDamage(Player, target, Damage.SummonerSpell.Ignite);
            var QDmg = Damage.GetSpellDamage(Player, target, SpellSlot.Q);
            var EDmg = Damage.GetSpellDamage(Player, target, SpellSlot.E);

            if (target.IsValidTarget())
            {
                if (Config.Item("UseIgnite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
                Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                {
                    if (IgniteDmg > target.Health)
                    {
                        Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
                    }
                }

                if (Config.Item("UseQKs").GetValue<bool>() && Q.IsReady()) {
                    if (QDmg >= target.Health) {
                        Q.Cast(target, PacketCast);
                    }
                }
                if (Config.Item("UseEKs").GetValue<bool>() && E.IsReady())
                {
                    if (EDmg >= target.Health)
                    {
                        E.Cast(target, PacketCast);
                    }
                }
            }
           
        }

        private static int GetEnemys(Obj_AI_Hero target) {
            int Enemys = 0;
            foreach(Obj_AI_Hero enemys in ObjectManager.Get<Obj_AI_Hero>()){

                var pred = R.GetPrediction(enemys, true);
                if(pred.Hitchance >= HitChance.High && !enemys.IsMe && enemys.IsEnemy && Vector3.Distance(Player.Position, pred.UnitPosition) <= R.Range){
                    Enemys = Enemys + 1;
                }
            }
        return Enemys;
        }

        private static void OnDraw(EventArgs args)
        {
            if (Config.Item("CircleLag").GetValue<bool>())
            {
                if (Config.Item("DrawQ").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.White,
                        Config.Item("CircleThickness").GetValue<Slider>().Value,
                        Config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (Config.Item("DrawE").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.White,
                        Config.Item("CircleThickness").GetValue<Slider>().Value,
                        Config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (Config.Item("DrawR").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.White,
                        Config.Item("CircleThickness").GetValue<Slider>().Value,
                        Config.Item("CircleQuality").GetValue<Slider>().Value);
                }
            }
            else
            {
                if (Config.Item("DrawQ").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.White);
                }
                if (Config.Item("DrawE").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.White);
                }
                if (Config.Item("DrawR").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.White);
                }

            }
        }
    }
}
