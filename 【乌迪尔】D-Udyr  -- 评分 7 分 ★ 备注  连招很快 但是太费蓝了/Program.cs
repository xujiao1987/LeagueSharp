

using System;
using System.Collections.Generic;
using System.Net;
using LeagueSharp;
using LeagueSharp.Common;
using System.Linq;


namespace D_Udyr
{
    internal class Program
    {

        public const string ChampionName = "Udyr";

        private static Orbwalking.Orbwalker _orbwalker;

        private static readonly List<Spell> SpellList = new List<Spell>();

        private static Spell _q;

        private static Spell _w;

        private static Spell _e;

        private static Spell _r;

        private static Menu _config;

        private static Obj_AI_Hero _player;

        private static SpellDataInst _smiteSlot;

        private static Items.Item _tiamat, _hydra, _blade, _bilge, _rand, _lotis;

        //Tiger Style
        private static List<int> Tiger = new List<int> {0, 1, 2, 0, 0, 2, 0, 2, 0, 2, 2, 1, 1, 1, 1, 3, 3, 3};
        //Phoenix Style
        private static List<int> Phoenix = new List<int> {3, 0, 2, 3, 3, 2, 3, 2, 3, 2, 2, 1, 0, 0, 0, 1, 1, 1};

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            //if (ObjectManager.Player.BaseSkinName != ChampionName) return;


            _q = new Spell(SpellSlot.Q, 200);
            _w = new Spell(SpellSlot.W, 200);
            _e = new Spell(SpellSlot.E, 200);
            _r = new Spell(SpellSlot.R, 200);

            SpellList.Add(_q);
            SpellList.Add(_w);
            SpellList.Add(_e);
            SpellList.Add(_r);

            _bilge = new Items.Item(3144, 475f);
            _blade = new Items.Item(3153, 425f);
            _hydra = new Items.Item(3074, 250f);
            _tiamat = new Items.Item(3077, 250f);
            _rand = new Items.Item(3143, 490f);
            _lotis = new Items.Item(3190, 590f);

            _smiteSlot = _player.SummonerSpellbook.GetSpell(_player.GetSpellSlot("summonersmite"));
            //Udyr
            _config = new Menu("D-Udyr", "D-Udyr", true);

            //TargetSelector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            _config.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            _config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            //Auto Level
            _config.AddSubMenu(new Menu("Style", "Style"));
            _config.SubMenu("Style").AddItem(new MenuItem("AutoLevel", "Auto Level")).SetValue(false);
            _config.SubMenu("Style")
                .AddItem(new MenuItem("Style", ""))
                .SetValue(new StringList(new string[2] {"Tiger", "Pheonix"}));


            //Combo
            _config.AddSubMenu(new Menu("Main", "Main"));
            _config.SubMenu("Main").AddItem(new MenuItem("AutoShield", "Auto Shield")).SetValue(true);
            _config.SubMenu("Main")
                .AddItem(new MenuItem("AutoShield%", "AutoShield HP %").SetValue(new Slider(50, 100, 0)));
            _config.SubMenu("Main")
                .AddItem(new MenuItem("TargetRange", "Range to Use E").SetValue(new Slider(1000, 600, 1500)));
            _config.SubMenu("Main")
                .AddItem(new MenuItem("ActiveCombo", "Combo Key").SetValue(new KeyBind(32, KeyBindType.Press)));
            _config.SubMenu("Main")
                .AddItem(
                    new MenuItem("StunCycle", "Stun Cycle").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));

            //Forest gump
            _config.AddSubMenu(new Menu("Forest Gump", "Forest Gump"));
            _config.SubMenu("Forest Gump").AddItem(new MenuItem("ForestE", "Use E")).SetValue(true);
            _config.SubMenu("Forest Gump").AddItem(new MenuItem("ForestW", "Use W")).SetValue(true);
            _config.SubMenu("Forest Gump")
                .AddItem(
                    new MenuItem("Forest", "Forest gump(Toggle)").SetValue(new KeyBind("G".ToCharArray()[0],
                        KeyBindType.Toggle)));
            _config.SubMenu("Forest Gump")
                .AddItem(new MenuItem("Forest-Mana", "Forest gump Mana").SetValue(new Slider(50, 100, 0)));

            _config.AddSubMenu(new Menu("items", "items"));
            _config.SubMenu("items").AddSubMenu(new Menu("Offensive", "Offensive"));
            _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Tiamat", "Use Tiamat")).SetValue(true);
            _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Hydra", "Use Hydra")).SetValue(true);
            _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Bilge", "Use Bilge")).SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("BilgeEnemyhp", "If Enemy Hp < ").SetValue(new Slider(85, 1, 100)));
            _config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("Bilgemyhp", "Or your Hp < ").SetValue(new Slider(85, 1, 100)));
            _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Blade", "Use Blade")).SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("BladeEnemyhp", "If Enemy Hp < ").SetValue(new Slider(85, 1, 100)));
            _config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("Blademyhp", "Or Your  Hp < ").SetValue(new Slider(85, 1, 100)));
            _config.SubMenu("items").AddSubMenu(new Menu("Deffensive", "Deffensive"));
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Omen", "Use Randuin Omen"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Omenenemys", "Randuin if enemys>").SetValue(new Slider(2, 1, 5)));
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("lotis", "Use Iron Solari"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("lotisminhp", "Solari if Ally Hp<  ").SetValue(new Slider(35, 1, 100)));



            //Farm
            _config.AddSubMenu(new Menu("Farm", "Farm"));
            _config.SubMenu("Farm").AddSubMenu(new Menu("Lane", "Lane"));
            _config.SubMenu("Farm").SubMenu("Lane").AddItem(new MenuItem("laneitems", "Use Items")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("Lane").AddItem(new MenuItem("Use-Q-Farm", "Use Q")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("Lane").AddItem(new MenuItem("Use-W-Farm", "Use W")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("Lane").AddItem(new MenuItem("Use-E-Farm", "Use E")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("Lane").AddItem(new MenuItem("Use-R-Farm", "Use R")).SetValue(true);
            _config.SubMenu("Farm")
                .SubMenu("Lane")
                .AddItem(new MenuItem("Farm-Mana", "Mana Limit").SetValue(new Slider(50, 100, 0)));
            _config.SubMenu("Farm")
                .SubMenu("Lane")
                .AddItem(
                    new MenuItem("ActiveLane", "Lane Key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            _config.SubMenu("Farm").AddSubMenu(new Menu("Jungle", "Jungle"));
            _config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("jungleitems", "Use Items")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("Use-Q-Jungle", "Use Q")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("Use-W-Jungle", "Use W")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("Use-E-Jungle", "Use E")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("Use-R-Jungle", "Use R")).SetValue(true);
            _config.SubMenu("Farm")
                .SubMenu("Jungle")
                .AddItem(new MenuItem("Jungle-Mana", "Mana Limit").SetValue(new Slider(50, 100, 0)));
            _config.SubMenu("Farm")
                .SubMenu("Jungle")
                .AddItem(
                    new MenuItem("ActiveJungle", "Jungle Key").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));

            //Smite 
            _config.AddSubMenu(new Menu("Smite", "Smite"));
            _config.SubMenu("Smite").AddItem(new MenuItem("Usesmite", "Use Smite(toggle)").SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Toggle)));
            _config.SubMenu("Smite").AddItem(new MenuItem("Useblue", "Smite Blue Early ")).SetValue(true);
            _config.SubMenu("Smite").AddItem(new MenuItem("manaJ", "Smite Blue Early if MP% <").SetValue(new Slider(35, 1, 100)));
            _config.SubMenu("Smite").AddItem(new MenuItem("Usered", "Smite Red Early ")).SetValue(true);
            _config.SubMenu("Smite").AddItem(new MenuItem("healthJ", "Smite Red Early if HP% <").SetValue(new Slider(35, 1, 100)));

            _config.AddToMainMenu();
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += OnGameUpdate;
            CustomEvents.Unit.OnLevelUp += OnLevelUp;


            Game.PrintChat("<font color='#881df2'>Udyr By Diabaths </font>Loaded!");
            Game.PrintChat("<font color='#881df2'>StunCycle by xcxooxl");
            Game.PrintChat(
                "<font color='#FF0000'>If You like my work and want to support, and keep it always up to date plz donate via paypal in </font> <font color='#FF9900'>ssssssssssmith@hotmail.com</font> (10) S");
        }

        private static void OnGameUpdate(EventArgs args)
        {

            _player = ObjectManager.Player;
            if (_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (_config.Item("StunCycle").GetValue<KeyBind>().Active)
            {
                StunCycle();
            }
            if (_config.Item("ActiveLane").GetValue<KeyBind>().Active &&
                (100*(_player.Mana/_player.MaxMana)) > _config.Item("Farm-Mana").GetValue<Slider>().Value)
            {
                Farm();
            }
            if (_config.Item("ActiveJungle").GetValue<KeyBind>().Active &&
                (100*(_player.Mana/_player.MaxMana)) > _config.Item("Jungle-Mana").GetValue<Slider>().Value)
            {
                JungleClear();
            }
            if (_config.Item("AutoShield").GetValue<bool>() && !_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                AutoW();
            }
            if (_config.Item("Forest").GetValue<KeyBind>().Active &&
                (100*(_player.Mana/_player.MaxMana)) > _config.Item("Forest-Mana").GetValue<Slider>().Value)
            {
                Forest();
            }
            if (_config.Item("Usesmite").GetValue<KeyBind>().Active)
            {
                Smiteuse();
            }

            _orbwalker.SetAttack(true);

            _orbwalker.SetMovement(true);
        }

        private static void OnLevelUp(LeagueSharp.Obj_AI_Base sender,
            LeagueSharp.Common.CustomEvents.Unit.OnLevelUpEventArgs args)
        {
            if (!sender.IsValid || !sender.IsMe)
                return;

            if (!_config.Item("AutoLevel").GetValue<bool>()) return;
            if (_config.Item("Style").GetValue<StringList>().SelectedIndex == 0)
                _player.Spellbook.LevelUpSpell((SpellSlot) Tiger[args.NewLevel - 1]);
            else if (_config.Item("Style").GetValue<StringList>().SelectedIndex == 1)
                _player.Spellbook.LevelUpSpell((SpellSlot) Phoenix[args.NewLevel - 1]);
        }


        private static void Farm()
        {
            var useItemsl = _config.Item("laneitems").GetValue<bool>();
            if (!Orbwalking.CanMove(40)) return;
            var minions = MinionManager.GetMinions(_player.ServerPosition, 500.0F);
            if (minions.Count < 3) return;


            if (_config.Item("Use-R-Farm").GetValue<bool>() && _r.IsReady())
            {
                _r.Cast();
            }
            if (_config.Item("Use-Q-Farm").GetValue<bool>() && _q.IsReady())
            {
                _q.Cast();
            }
            if (_config.Item("Use-W-Farm").GetValue<bool>() && _w.IsReady())
            {
                _w.Cast();
            }
            if (_config.Item("Use-E-Farm").GetValue<bool>() && _e.IsReady())
            {
                _e.Cast();
            }
            if (useItemsl && _hydra.IsReady())
            {
                foreach (var minion in minions)
                {
                    if (_player.Distance(minion) < _hydra.Range)
                    {
                        _hydra.Cast();
                    }
                }
            }
            if (useItemsl && _tiamat.IsReady())
            {
                foreach (var minion in minions)
                {
                    if (_player.Distance(minion) < _tiamat.Range)
                    {
                        _tiamat.Cast();
                    }
                }
            }
        }


        private static void Forest()
        {
            if (_player.HasBuff("Recall")) return;

            if (_e.IsReady() && _config.Item("ForestE").GetValue<bool>())
            {
                _e.Cast();
            }
            if (_w.IsReady() && _config.Item("ForestW").GetValue<bool>())
            {
                _w.Cast();
            }
        }

        private static void AutoW()
        {
            if (_w.IsReady())
            {
                if (_player.HasBuff("Recall")) return;
                if (Utility.CountEnemysInRange(1000) >= 1 &&
                    _player.Health <= (_player.MaxHealth*(_config.Item("AutoShield%").GetValue<Slider>().Value)/100))
                {
                    _w.Cast();
                }
            }
        }

        private static void JungleClear()
        {
            if (!Orbwalking.CanMove(40)) return;
            var useitems = _config.Item("jungleitems").GetValue<bool>();
            var minions = MinionManager.GetMinions(_player.ServerPosition, 400, MinionTypes.All, MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);

            if (useitems && _hydra.IsReady())
            {
                foreach (var minion in minions)
                {
                    if (_player.Distance(minion) < _hydra.Range)
                    {
                        _hydra.Cast();
                    }
                }
            }
            if (useitems && _tiamat.IsReady())
            {
                foreach (var minion in minions)
                {
                    if (_player.Distance(minion) < _tiamat.Range)
                    {
                        _tiamat.Cast();
                    }
                }
            }
            if (_config.Item("Use-Q-Jungle").GetValue<bool>() && _q.IsReady())
            {

                foreach (var minion in minions)
                {
                    if (minion.IsValidTarget())
                    {

                        _q.Cast();
                        return;
                    }
                }
            }

            else if (_config.Item("Use-R-Jungle").GetValue<bool>() && _r.IsReady())
            {

                foreach (var minion in minions)
                {
                    if (minion.IsValidTarget())
                    {

                        _r.Cast();
                        return;
                    }
                }
            }
            else if (_config.Item("Use-W-Jungle").GetValue<bool>() && _w.IsReady())
            {

                foreach (var minion in minions)
                {
                    if (minion.IsValidTarget())
                    {

                        _w.Cast();
                        return;
                    }
                }
            }
            else if (_config.Item("Use-E-Jungle").GetValue<bool>() && _e.IsReady())
            {

                foreach (var minion in minions)
                {
                    if (minion.IsValidTarget())
                    {

                        _e.Cast();
                        return;
                    }
                }
            }

        }

        private static void UseItemes(Obj_AI_Hero target)
        {
            var iBilge = _config.Item("Bilge").GetValue<bool>();
            var iBilgeEnemyhp = target.Health <=
                                (target.MaxHealth*(_config.Item("BilgeEnemyhp").GetValue<Slider>().Value)/100);
            var iBilgemyhp = _player.Health <=
                             (_player.MaxHealth*(_config.Item("Bilgemyhp").GetValue<Slider>().Value)/100);
            var iBlade = _config.Item("Blade").GetValue<bool>();
            var iBladeEnemyhp = target.Health <=
                                (target.MaxHealth*(_config.Item("BladeEnemyhp").GetValue<Slider>().Value)/100);
            var iBlademyhp = _player.Health <=
                             (_player.MaxHealth*(_config.Item("Blademyhp").GetValue<Slider>().Value)/100);
            var iOmen = _config.Item("Omen").GetValue<bool>();
            var iOmenenemys = ObjectManager.Get<Obj_AI_Hero>().Count(hero => hero.IsValidTarget(450)) >=
                              _config.Item("Omenenemys").GetValue<Slider>().Value;
            var iTiamat = _config.Item("Tiamat").GetValue<bool>();
            var iHydra = _config.Item("Hydra").GetValue<bool>();
            var ilotis = _config.Item("lotis").GetValue<bool>();
            //var ihp = _config.Item("Hppotion").GetValue<bool>();
            // var ihpuse = _player.Health <= (_player.MaxHealth * (_config.Item("Hppotionuse").GetValue<Slider>().Value) / 100);
            //var imp = _config.Item("Mppotion").GetValue<bool>();
            //var impuse = _player.Health <= (_player.MaxHealth * (_config.Item("Mppotionuse").GetValue<Slider>().Value) / 100);

            if (_player.Distance(target) <= 450 && iBilge && (iBilgeEnemyhp || iBilgemyhp) && _bilge.IsReady())
            {
                _bilge.Cast(target);

            }
            if (_player.Distance(target) <= 450 && iBlade && (iBladeEnemyhp || iBlademyhp) && _blade.IsReady())
            {
                _blade.Cast(target);

            }
            if (iTiamat && _tiamat.IsReady() && target.IsValidTarget(_tiamat.Range))
            {
                _tiamat.Cast();

            }
            if (iHydra && _hydra.IsReady() && target.IsValidTarget(_hydra.Range))
            {
                _hydra.Cast();

            }
            if (iOmenenemys && iOmen && _rand.IsReady())
            {
                _rand.Cast();

            }
            if (ilotis)
            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly || hero.IsMe))
                {
                    if (hero.Health <= (hero.MaxHealth*(_config.Item("lotisminhp").GetValue<Slider>().Value)/100) &&
                        hero.Distance(_player.ServerPosition) <= _lotis.Range && _lotis.IsReady())
                        _lotis.Cast();
                }
            }

        }

        private static void Combo()
        {
            //Create target

            var target = SimpleTs.GetTarget(_config.Item("TargetRange").GetValue<Slider>().Value,
                SimpleTs.DamageType.Magical);

            if (target != null && _player.Distance(target) <= _config.Item("TargetRange").GetValue<Slider>().Value)
            {
                if (_e.IsReady() && !target.HasBuff("udyrbearstuncheck", true))
                {
                    _e.Cast();
                    return;
                }
                if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level >=
                    ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Level)
                    if (_q.Cast()) return;

                if (_r.IsReady() && target.HasBuff("udyrbearstuncheck", true))
                {
                    _r.Cast();
                    return;
                }

                if (_q.IsReady() && target.HasBuff("udyrbearstuncheck", true))
                {
                    _q.Cast();
                    return;
                }

                if (_w.IsReady() && target.HasBuff("udyrbearstuncheck", true))
                {
                    _w.Cast();
                    return;
                }
                UseItemes(target);
            }
        }

        private static void StunCycle()
        {
            Obj_AI_Hero closestEnemy = null;

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsValidTarget(800) && !enemy.HasBuff("udyrbearstuncheck"))
                {
                    if (_e.IsReady())
                    {
                        _e.Cast();
                    }
                    if (closestEnemy == null)
                    {
                        closestEnemy = enemy;
                    }
                    else if (_player.Distance(closestEnemy) < _player.Distance(enemy))
                    {
                        closestEnemy = enemy;
                    }
                    else if (enemy.HasBuff("udyrbearstuncheck"))
                    {
                        Game.PrintChat(closestEnemy.BaseSkinName + " has buff already !!!");
                        closestEnemy = enemy;
                        Game.PrintChat(enemy.BaseSkinName + "is the new target");

                    }
                    if (!enemy.HasBuff("udyrbearstuncheck"))
                    {
                        _player.IssueOrder(GameObjectOrder.AttackUnit, closestEnemy);
                    }

                }
            }
        }

        private static int GetSmiteDmg()
        {
            int level = _player.Level;
            int index = _player.Level/5;
            float[] dmgs = {370 + 20*level, 330 + 30*level, 240 + 40*level, 100 + 50*level};
            return (int) dmgs[index];
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_config.Item("Usesmite").GetValue<KeyBind>().Active)
            {
                Drawing.DrawText(Drawing.Width*0.90f, Drawing.Height*0.68f, System.Drawing.Color.DarkOrange,
                    "Smite Is On");
            }
            else
                Drawing.DrawText(Drawing.Width*0.90f, Drawing.Height*0.68f, System.Drawing.Color.DarkRed,
                    "Smite Is Off");
            if (_config.Item("Forest").GetValue<KeyBind>().Active)
            {
                Drawing.DrawText(Drawing.Width*0.90f, Drawing.Height*0.66f, System.Drawing.Color.DarkOrange,
                    "Forest Is On");
            }
            else
                Drawing.DrawText(Drawing.Width*0.90f, Drawing.Height*0.66f, System.Drawing.Color.DarkRed,
                    "Forest Is Off");
        }

        //New map Monsters Name By SKO
        private static void Smiteuse()
        {
            var jungleMinions = new string[]
            {
                "TT_Spiderboss", "TTNGolem", "TTNWolf", "TTNWraith",
                "SRU_Blue", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Red", "SRU_Krug", "SRU_Dragon",
                "SRU_Baron", "Sru_Crab"
            };
            var useblue = _config.Item("Useblue").GetValue<bool>();
            var usered = _config.Item("Usered").GetValue<bool>();
            var junglesmite = _config.Item("ActiveJungle").GetValue<KeyBind>().Active;
            var health = (100 * (_player.Mana / _player.MaxMana)) < _config.Item("healthJ").GetValue<Slider>().Value;
            var mana = (100 * (_player.Mana / _player.MaxMana)) < _config.Item("manaJ").GetValue<Slider>().Value;
            //var health = _player.Health <= (_player.MaxHealth*20/100);
            //var mana = _player.Mana <= (_player.MaxMana*20/100);
            var minions = MinionManager.GetMinions(_player.Position, 1000, MinionTypes.All, MinionTeam.Neutral);
            if (minions.Count() > 0)
            {
                int smiteDmg = GetSmiteDmg();
                foreach (Obj_AI_Base minion in minions)
                {
                    if (minion.Health <= smiteDmg && jungleMinions.Any(name => minion.Name.StartsWith(name)) &&
                        !jungleMinions.Any(name => minion.Name.Contains("Mini")) &&
                        ObjectManager.Player.SummonerSpellbook.CanUseSpell(_smiteSlot.Slot) == SpellState.Ready)
                    {
                        _player.SummonerSpellbook.CastSpell(_smiteSlot.Slot, minion);
                    }
                    else if (junglesmite && useblue &&
                             ObjectManager.Player.SummonerSpellbook.CanUseSpell(_smiteSlot.Slot) == SpellState.Ready &&
                             mana && minion.Health >= smiteDmg &&
                             jungleMinions.Any(name => minion.Name.StartsWith("SRU_Blue")) &&
                             !jungleMinions.Any(name => minion.Name.Contains("Mini")))
                    {
                        _player.SummonerSpellbook.CastSpell(_smiteSlot.Slot, minion);
                    }
                    else if (junglesmite && usered &&
                             ObjectManager.Player.SummonerSpellbook.CanUseSpell(_smiteSlot.Slot) == SpellState.Ready &&
                             health && minion.Health >= smiteDmg &&
                             jungleMinions.Any(name => minion.Name.StartsWith("SRU_Red")) &&
                             !jungleMinions.Any(name => minion.Name.Contains("Mini")))
                    {
                        _player.SummonerSpellbook.CastSpell(_smiteSlot.Slot, minion);
                    }
                }
            }
        }
    }
}


   