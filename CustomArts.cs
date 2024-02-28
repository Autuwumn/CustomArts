using BepInEx;
using HarmonyLib;
using Jotunn.Utils;
using UnityEngine;
using UnboundLib;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BepInEx.Configuration;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;
using UnboundLib.Utils.UI;
using TMPro;

namespace CustomArts
{
    [BepInDependency("com.willis.rounds.unbound")]
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class CustomArts : BaseUnityPlugin
    {
        private const string ModId = "koala.rounds.customarts";
        private const string ModName = "Custom Arts";
        private const string Version = "0.0.0";

        public static CustomArts instance;

        private List<GameObject> Foregrounds = new List<GameObject>();
        private List<GameObject> Backgrounds = new List<GameObject>();

        private List<ArtThing> ForeConfig = new List<ArtThing>();
        private List<ArtThing> BackConfig = new List<ArtThing>();

        public class ArtThing
        {
            public ConfigEntry<bool> ConfigEntry;
            public string
                name,
                modid,
                modname;
        }

        private GameObject FunnyStorer;

        public int currentFg = 0;
        public int currentBg = 0;

        private void NewGUI(GameObject menu)
        {
            string tempName;
            string tempModId;

            var a = MenuHandler.CreateMenu("Foregrounds", () => { }, menu);
            List<string> mods = new List<string>();
            foreach (var i in ForeConfig)
            {
                if (!mods.Contains(i.modname)) mods.Add(i.modname);
            }
            foreach (var m in mods)
            {
                var b = MenuHandler.CreateMenu(m, () => { }, a, 50);
                foreach (var i in ForeConfig.Where((z) => z.modname == m))
                {
                    MenuHandler.CreateToggle(i.ConfigEntry.Value, i.name, b, (bool val) =>
                    {
                        ForeConfig.Where((a) => a.name == i.name && a.modid == i.modid).First().ConfigEntry.Value = val;
                    });
                }
            }

            var c = MenuHandler.CreateMenu("Backgrounds", () => { }, menu);
            List<string> modsc = new List<string>();
            foreach (var i in BackConfig)
            {
                if (!modsc.Contains(i.modname)) modsc.Add(i.modname);
            }
            foreach (var m in modsc)
            {
                var d = MenuHandler.CreateMenu(m, () => { }, c, 50);
                foreach (var i in BackConfig.Where((z) => z.modname == m))
                {
                    tempName = i.name;
                    tempModId = i.modid;
                    MenuHandler.CreateToggle(i.ConfigEntry.Value, i.name, d, (bool val) =>
                    {
                        BackConfig.Where((a) => a.name == i.name && a.modid == i.modid).First().ConfigEntry.Value = val;
                    });
                }
            }
        }

        private void Awake()
        {
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
            if (!instance) instance = this; else Destroy(this);

            instance.FunnyStorer = Instantiate(new GameObject());
            instance.FunnyStorer.name = "CustomArts - Art Storer";
            DontDestroyOnLoad(instance.FunnyStorer);

            Unbound.RegisterClientSideMod(ModId);
        }
        private void Start()
        {
            for (var i = 0; i < GameObject.Find("FrontParticles").transform.childCount; i++)
            {
                if (i == 9) continue;
                var fp = GameObject.Find("FrontParticles").transform.GetChild(i);
                if (fp.childCount != 0)
                {
                    for (var j = 0; j < fp.childCount; j++)
                    {
                        fp.GetChild(j).gameObject.SetActive(true);
                    }
                }
                RegisterFG(fp.gameObject, ModId, ModName);
            }
            for (var i = 0; i < GameObject.Find("BackgroudParticles").transform.childCount; i++)
            {
                var flask = GameObject.Find("BackgroudParticles").transform.GetChild(i);
                RegisterBG(flask.gameObject, ModId, ModName);
            }
            for (var i = 0; i < GameObject.Find("UNUSED").transform.childCount; i++)
            {
                var ps = GameObject.Find("UNUSED").transform.GetChild(i);
                RegisterHFG(ps.gameObject, ModId, ModName);
            }


            Unbound.RegisterMenu("Custom Arts", () => { }, NewGUI, null, false);
        }
        public void LoadArt(int fgid, int bgid)
        {
            instance.TurnArtsOff();
            if (fgid >= instance.Foregrounds.Count || fgid < 0) fgid = Random.Range(0, instance.Foregrounds.Count);
            if (bgid >= instance.Backgrounds.Count || bgid < 0) bgid = Random.Range(0, instance.Backgrounds.Count);
            var fg = instance.Foregrounds[fgid].gameObject;
            fg.SetActive(true);
            if(fg.transform.childCount != 0) for (int i = 0; i < fg.transform.childCount; i++) fg.transform.GetChild(i).gameObject.SetActive(true);
            if (fg.GetComponent<ParticleSystem>()) fg.GetComponent<ParticleSystem>().Play();
            fg.gameObject.GetComponent<ParticleSystemRenderer>().sortingLayerName = "MapParticle";
            var bg = instance.Backgrounds[bgid].gameObject;
            bg.SetActive(true);
            if (bg.transform.childCount != 0) for (int i = 0; i < bg.transform.childCount; i++) bg.transform.GetChild(i).gameObject.SetActive(true);
            if (bg.GetComponent<ParticleSystem>()) bg.GetComponent<ParticleSystem>().Play();
            bg.gameObject.GetComponent<ParticleSystemRenderer>().sortingLayerName = "MapParticle";
            instance.currentFg = fgid;
            instance.currentBg = bgid;
        }
        public void RegisterBG(GameObject obje, string modid, string modname)
        {
            var obj = Instantiate(obje, instance.FunnyStorer.transform);
            instance.Backgrounds.Add(obj);
            var newArtConfig = new ArtThing()
            {
                name = obje.name,
                modid = modid,
                modname = modname,
                ConfigEntry = Config.Bind("CustomArts", modid + "_BG_" + obje.name, true)
            };
            BackConfig.Add(newArtConfig);
            obj.name = modname + "_" + obje.name;
            obj.SetActive(false);
        }
        public void RegisterFG(GameObject obje, string modid, string modname)
        {
            var obj = Instantiate(obje, instance.FunnyStorer.transform);
            instance.Foregrounds.Add(obj);
            var newArtConfig = new ArtThing()
            {
                name = obje.name,
                modid = modid,
                modname = modname,
                ConfigEntry = Config.Bind("CustomArts", modid + "_FG_" + obje.name, true)
            };
            ForeConfig.Add(newArtConfig);
            obj.name = modname + "_" + obje.name;
            obj.SetActive(false);
        }
        private void RegisterHFG(GameObject obje, string modid, string modname)
        {
            var obj = Instantiate(obje, instance.FunnyStorer.transform);
            instance.Foregrounds.Add(obj);
            var newArtConfig = new ArtThing()
            {
                name = obje.name,
                modid = modid,
                modname = modname,
                ConfigEntry = Config.Bind("CustomArts", modid + "_FG_" + obje.name, false)
            };
            ForeConfig.Add(newArtConfig);
            obj.name = modname + "_" + obje.name;
            obj.SetActive(false);
        }
        public void TurnArtsOff()
        {
            foreach (var go in instance.Backgrounds) go.SetActive(false);
            foreach (var go in instance.Foregrounds) go.SetActive(false);
        }
        public void LoadRandomArt()
        {
            instance.TurnArtsOff();
            var fgid = 0;
            var fg = instance.Foregrounds[0];
            if (ForeConfig.Where((f) => f.ConfigEntry.Value == true).ToList().Count != 0)
            {
                var chosenFront = ForeConfig.Where((f) => f.ConfigEntry.Value == true).ToList()[Random.Range(0, ForeConfig.Where((f) => f.ConfigEntry.Value == true).ToList().Count)];
                string randomAllowedForeground = chosenFront.modname+"_"+chosenFront.name;
                fg = instance.Foregrounds.Where((g) => g.name == randomAllowedForeground).First();
                fgid = instance.Foregrounds.IndexOf(fg);
            }
            else
            {
                fg = instance.Foregrounds[15];
            }
            var bgid = 0;
            var bg = instance.Backgrounds[0];
            if (BackConfig.Where((f) => f.ConfigEntry.Value == true).ToList().Count != 0)
            {
                var chosenBack = BackConfig.Where((f) => f.ConfigEntry.Value == true).ToList()[Random.Range(0, BackConfig.Where((f) => f.ConfigEntry.Value == true).ToList().Count)];
                string randomAllowedBackground = chosenBack.modname+"_"+chosenBack.name;
                bg = instance.Backgrounds.Where((g) => g.name == randomAllowedBackground).First();
                bgid = instance.Backgrounds.IndexOf(fg);
            }
            else
            {
                bg = instance.Backgrounds[4];
            }

            fg.SetActive(true);
            if (fg.transform.childCount != 0) for (int i = 0; i < fg.transform.childCount; i++) { fg.transform.GetChild(i).gameObject.SetActive(true); }
            if (fg.GetComponent<ParticleSystem>())
            {
                fg.GetComponent<ParticleSystem>().Play();
                fg.gameObject.GetComponent<ParticleSystemRenderer>().sortingLayerName = "MapParticle";
            }

                bg.SetActive(true);
            if (bg.transform.childCount != 0) for (int i = 0; i < bg.transform.childCount; i++) bg.transform.GetChild(i).gameObject.SetActive(true);

            if (bg.GetComponent<ParticleSystem>())
            {
                bg.GetComponent<ParticleSystem>().Play(); bg.gameObject.GetComponent<ParticleSystemRenderer>().sortingLayerName = "MapParticle";
            }

            instance.currentFg = fgid;
            instance.currentBg = bgid;
        }
    }
}
