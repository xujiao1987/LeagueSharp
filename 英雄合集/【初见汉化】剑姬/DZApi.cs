using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
namespace FioraRaven
{
    class DZApi
    {
        private Dictionary<String, String> dSpellsName = new Dictionary<String, String>();
        private Dictionary<int,String> itemNames = new  Dictionary<int,String>();
        public static Obj_AI_Base player = ObjectManager.Player;
        private string[] dSpellsNames;
        public DZApi()
        {
            fillDSpellList();
        }
        public Dictionary<String,String> getDanSpellsName()
        {
            return dSpellsName;
        }
         public Dictionary<int,String> getItemNames()
        {
            return itemNames;
        }
        public void addSpell(String name,String DisplayName)
        {
            dSpellsName.Add(name, DisplayName);
        }
        public void fillDSpellList()
        {
            addSpell("CurseofTheSadMummy", "闃挎湪鏈▅ R");
            addSpell("InfernalGuardian", "瀹夊Ξ| R");
            addSpell("BlindMonkRKick", "鐩插儳| R");
            addSpell("GalioIdolOfDurand", "鍝ㄥ叺涔嬫 R");
            addSpell("syndrar", "杈涘痉鎷墊 R");
            addSpell("BusterShot", "灏忕偖| R");
            addSpell("UFSlash", "鐭冲ご浜簗 R");
            addSpell("VeigarPrimordialBurst", "灏忔硶| R");
            addSpell("ViR", "钄殀 R");
            addSpell("AlZaharNetherGrasp", "椹皵鎵庡搱| R");
        }
        public float getEnH(Obj_AI_Hero target)
        {
            float h = (target.Health / target.MaxHealth) * 100;
            return h;
        }
        public float getManaPer()
        {
            float mana = (player.Mana / player.MaxMana) * 100;
            return mana;
        }
        public float getPlHPer()
        {
            float h = (player.Health / player.MaxHealth) * 100;
            return h;
        }
        public void useItem(int id, Obj_AI_Hero target = null)
        {
            if (Items.HasItem(id) && Items.CanUseItem(id))
            {
                Items.UseItem(id, target);
            }
        }
    }
}
