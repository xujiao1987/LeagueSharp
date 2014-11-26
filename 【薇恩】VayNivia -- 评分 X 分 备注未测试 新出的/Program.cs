#region LICENSE

// Copyright 2014 - 2014 VayNivia
// Program.cs is part of VayNivia.
// VayNivia is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// VayNivia is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with VayNivia. If not, see <http://www.gnu.org/licenses/>.

#endregion

#region

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using VayNivia.Properties;

#endregion

namespace VayNivia
{
    public class Program
    {
        public static Spell Wall = new Spell(SpellSlot.W, 1000);
        public static Spell Condemn = new Spell(SpellSlot.E, 550);
        public static Menu Config = new Menu("VayNivia", "VayNivia", true);

        public static bool CondemnKey
        {
            get { return Config.Item("Condemn.Key").GetValue<KeyBind>().Active; }
        }

        public static bool ComboCheck(Obj_AI_Hero target)
        {
            if (target == null || target.IsDead)
                return false;

            var anivia = ObjectManager.Get<Obj_AI_Hero>().SingleOrDefault(h => h.IsAlly && h.ChampionName == "Anivia");
            var vayne = ObjectManager.Get<Obj_AI_Hero>().SingleOrDefault(h => h.IsAlly && h.ChampionName == "Vayne");

            if (vayne == null || anivia == null)
                return false;

            var condemn = vayne.Spellbook.GetSpell(SpellSlot.E);
            var wall = anivia.Spellbook.GetSpell(SpellSlot.W);
            var wallPos = target.Position.To2D().Extend(vayne.ServerPosition.To2D(), -100).To3D();

            return wall.CooldownExpires < Game.Time && condemn.CooldownExpires < Game.Time &&
                   wall.ManaCost < anivia.Mana && condemn.ManaCost < vayne.Mana && anivia.Distance(wallPos) < 1000 &&
                   vayne.Distance(target) < 550;
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += eventArgs =>
            {
                if (
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(h => h.IsAlly)
                        .Count(h => h.ChampionName == "Anivia" || h.ChampionName == "Vayne") == 2)
                {
                    foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsEnemy))
                    {
                        var hero1 = hero;

                        var sprite = new Render.Sprite(Resources.Crystallize, hero.HPBarPosition)
                        {
                            Scale = new Vector2(0.3f, 0.3f)
                        };

                        sprite.PositionUpdate += () => new Vector2(hero1.HPBarPosition.X + 145, hero1.HPBarPosition.Y + 20);
                        sprite.VisibleCondition += s => ComboCheck(hero1);
                        sprite.Add();
                    }

                    if (ObjectManager.Player.ChampionName == "Anivia")
                    {
                        Obj_AI_Base.OnProcessSpellCast += AniviaIntegration;
                        Extensions.PrintMessage("Aniva by h3h3 loaded.");
                    }

                    if (ObjectManager.Player.ChampionName == "Vayne")
                    {
                        Config.AddItem(
                            new MenuItem("Condemn.Key", "Condemn Key").SetValue(new KeyBind(32, KeyBindType.Press)));
                        Config.AddToMainMenu();

                        Game.OnGameUpdate += VayneIntegration;
                        Extensions.PrintMessage("Vayne by h3h3 loaded.");
                    }
                }
            };
        }

        private static void VayneIntegration(EventArgs args)
        {
            try
            {
                if (!Condemn.IsReady() || ObjectManager.Player.IsDead || !CondemnKey)
                    return;

                var anivia =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .SingleOrDefault(h => h.IsAlly && !h.IsDead && h.ChampionName == "Anivia");
                var target = SimpleTs.GetTarget(Condemn.Range, SimpleTs.DamageType.Physical);

                if (ComboCheck(target))
                {
                    var condemnEndPos =
                        target.ServerPosition.To2D()
                            .Extend(ObjectManager.Player.ServerPosition.To2D(), -150)
                            .To3D();

                    if (anivia.Distance(condemnEndPos) < 990)
                    {
                        Condemn.CastOnUnit(target, true);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void AniviaIntegration(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            try
            {
                if (!Wall.IsReady() || ObjectManager.Player.IsDead)
                    return;

                if (!sender.IsValid<Obj_AI_Hero>() || !args.Target.IsValid<Obj_AI_Hero>() || sender.IsEnemy ||
                    args.SData.Name != "VayneCondemn")
                    return;

                var condemnEndPos = args.Target.Position.To2D().Extend(sender.ServerPosition.To2D(), -450).To3D();
                var wallPos = args.Target.Position.To2D().Extend(sender.ServerPosition.To2D(), -100).To3D();

                if (!NavMesh.GetCollisionFlags(condemnEndPos).HasFlag(CollisionFlags.Wall | CollisionFlags.Building) &&
                    Wall.IsInRange(wallPos))
                {
                    Wall.Cast(wallPos, true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    internal static class Extensions
    {
        public static bool IsValid<T>(this GameObject obj)
        {
            return obj.IsValid && obj is T;
        }

        public static bool IsInRange(this Spell spell, Vector3 pos)
        {
            return ObjectManager.Player.Distance(pos) < spell.Range;
        }

        public static void PrintMessage(string message)
        {
            Game.PrintChat("<font color='#15C3AC'>VayNivia:</font> <font color='#FFFFFF'>" + message + "</font>");
        }
    }
}