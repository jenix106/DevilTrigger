using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace DevilTrigger
{
    public class MusicModule : ThunderScript
    {
        public static float score;
        List<WaveSpawner> spawners = new List<WaveSpawner>();
        [ModOption(name: "Enable/Disable", tooltip: "Enables/disables the dynamic music", valueSourceName: nameof(announcerValues), defaultValueIndex = 0, order = 0)]
        public static bool EnableMusic = true;
        [ModOption(name: "D Rank Score", tooltip: "The score needed to get to D Rank. Max: 1000", valueSourceName: nameof(scoreValues), category = "Score Values", defaultValueIndex = 5, order = 2)]
        public static float DRankScore = 50;
        [ModOption(name: "C Rank Score", tooltip: "The score needed to get to C Rank. Max: 1000", valueSourceName: nameof(scoreValues), category = "Score Values", defaultValueIndex = 10, order = 3)]
        public static float CRankScore = 100;
        [ModOption(name: "B Rank Score", tooltip: "The score needed to get to B Rank. Max: 1000", valueSourceName: nameof(scoreValues), category = "Score Values", defaultValueIndex = 15, order = 4)]
        public static float BRankScore = 150;
        [ModOption(name: "A Rank Score", tooltip: "The score needed to get to A Rank. Max: 1000", valueSourceName: nameof(scoreValues), category = "Score Values", defaultValueIndex = 20, order = 5)]
        public static float ARankScore = 200;
        [ModOption(name: "S Rank Score", tooltip: "The score needed to get to S Rank. Max: 1000", valueSourceName: nameof(scoreValues), category = "Score Values", defaultValueIndex = 25, order = 6)]
        public static float SRankScore = 250;
        [ModOption(name: "SS Rank Score", tooltip: "The score needed to get to SS Rank. Max: 1000", valueSourceName: nameof(scoreValues), category = "Score Values", defaultValueIndex = 30, order = 7)]
        public static float SSRankScore = 300;
        [ModOption(name: "SSS Rank Score", tooltip: "The score needed to get to SSS Rank. Max: 1000", valueSourceName: nameof(scoreValues), category = "Score Values", defaultValueIndex = 35, order = 8)]
        public static float SSSRankScore = 350;
        [ModOption(name: "Max Score", tooltip: "The max score. Max: 1000", valueSourceName: nameof(scoreValues), category = "Score Values", defaultValueIndex = 40, order = 1)]
        public static float MaxScore = 400;
        [ModOption(name: "Announcer", tooltip: "Enables/disables the rank announcer.", valueSourceName: nameof(announcerValues), defaultValueIndex = 0, order = 1)]
        public static bool Announcer = true;
        [ModOption(name: "Damage Max Bonus", tooltip: "The max bonus you can gain by hitting an enemy, based on damage. Max: 100", valueSourceName: nameof(gainValues), category = "Action Bonus", defaultValueIndex = 5)]
        public static float OnHitMaxScoreBonus = 5;
        [ModOption(name: "Kill Bonus", tooltip: "The bonus you gain by killing an enemy. Max: 100", valueSourceName: nameof(gainValues), category = "Action Bonus", defaultValueIndex = 5)]
        public static float OnKillScoreBonus = 5;
        [ModOption(name: "Parry Bonus", tooltip: "The bonus you gain by parrying an attack. Max: 100", valueSourceName: nameof(gainValues), category = "Action Bonus", defaultValueIndex = 5)]
        public static float OnParryScoreBonus = 5;
        [ModOption(name: "Dismember Bonus", tooltip: "The bonus you gain by dismembering a living enemy. Max: 100", valueSourceName: nameof(gainValues), category = "Action Bonus", defaultValueIndex = 15)]
        public static float OnDismemberScoreBonus = 15;
        public static ModOptionBool[] announcerValues =
        {
            new ModOptionBool("Enabled", true),
            new ModOptionBool("Disabled", false)
        };
        public static ModOptionFloat[] scoreValues()
        {
            ModOptionFloat[] modOptionFloats = new ModOptionFloat[101];
            float num = 0f;
            for (int i = 0; i < modOptionFloats.Length; ++i)
            {
                modOptionFloats[i] = new ModOptionFloat(num.ToString("0"), num);
                num += 10;
            }
            return modOptionFloats;
        }
        public static ModOptionFloat[] gainValues()
        {
            ModOptionFloat[] modOptionFloats = new ModOptionFloat[101];
            float num = 0f;
            for (int i = 0; i < modOptionFloats.Length; ++i)
            {
                modOptionFloats[i] = new ModOptionFloat(num.ToString("0"), num);
                num += 1;
            }
            return modOptionFloats;
        }
        public override void ScriptEnable()
        {
            base.ScriptEnable();
            EventManager.onCreatureHit += EventManager_onCreatureHit;
            EventManager.onCreatureKill += EventManager_onCreatureKill;
            EventManager.onCreatureParry += EventManager_onCreatureParry;
            EventManager.onCreatureSpawn += EventManager_onCreatureSpawn;
        }

        private void EventManager_onCreatureSpawn(Creature creature)
        {
            creature.ragdoll.OnSliceEvent += Ragdoll_OnSliceEvent;
        }

        private void Ragdoll_OnSliceEvent(RagdollPart ragdollPart, EventTime eventTime)
        {
            if (ragdollPart.ragdoll.creature != Player.local.creature && eventTime == EventTime.OnStart && !ragdollPart.ragdoll.creature.isKilled && !ragdollPart.isSliced)
            {
                score += OnDismemberScoreBonus;
            }
        }

        public override void ScriptDisable()
        {
            base.ScriptDisable();
            EventManager.onCreatureHit -= EventManager_onCreatureHit;
            EventManager.onCreatureKill -= EventManager_onCreatureKill;
            EventManager.onCreatureParry -= EventManager_onCreatureParry;
            EventManager.onCreatureSpawn -= EventManager_onCreatureSpawn;
            foreach (Creature creature in Creature.all)
            {
                creature.ragdoll.OnSliceEvent -= Ragdoll_OnSliceEvent;
            }
        }
        public override void ScriptUpdate()
        {
            base.ScriptUpdate();
            foreach (WaveSpawner spawner in WaveSpawner.instances)
            {
                if (spawner != null && !spawners.Contains(spawner))
                {
                    spawner.gameObject.AddComponent<MusicComponent>();
                    spawners.Add(spawner);
                }
            }
            if (score > MaxScore) score = MaxScore;
            score -= Time.deltaTime;
            if (score < 0) score = 0;
        }

        private void EventManager_onCreatureParry(Creature creature, CollisionInstance collisionInstance)
        {
            if (creature != Player.local.creature)
            {
                score += OnParryScoreBonus;
            }
        }

        private void EventManager_onCreatureKill(Creature creature, Player player, CollisionInstance collisionInstance, EventTime eventTime)
        {
            if (eventTime == EventTime.OnEnd)
            {
                if (creature != Player.local.creature)
                {
                    score += OnKillScoreBonus;
                }
                if (creature == Player.local.creature)
                {
                    score = 0;
                }
            }
        }

        private void EventManager_onCreatureHit(Creature creature, CollisionInstance collisionInstance, EventTime eventTime)
        {
            if (eventTime == EventTime.OnEnd)
            {
                if (creature != Player.local.creature && !creature.isKilled)
                {
                    score += Mathf.Clamp(collisionInstance.damageStruct.damage, 0, OnHitMaxScoreBonus);
                }
                if (creature == Player.local.creature && collisionInstance.damageStruct.damage > 0 && !collisionInstance.ignoreDamage)
                {
                    score *= 0.5f;
                }
            }
        }
    }
    public class MusicComponent : MonoBehaviour
    {
        WaveSpawner spawner;
        string[] strings = {"Jenix.DTOpening", "Jenix.DTDRankTransition", "Jenix.DTDRank", "Jenix.DTEnding", "Jenix.DTSRankTransition", "Jenix.DTSRank",
            "Jenix.AAnnouncer", "Jenix.BAnnouncer", "Jenix.CAnnouncer", "Jenix.DAnnouncer", "Jenix.SAnnouncer", "Jenix.SSAnnouncer", "Jenix.SSSAnnouncer"};
        Dictionary<string, AudioContainer> audioContainers = new Dictionary<string, AudioContainer>();
        List<AudioSource> audioSources = new List<AudioSource>();
        AudioSource announcer;
        AudioSource step;
        double nextEventTime;
        int flip = 1;
        int index = 0;
        public float beatEnd = 6f;
        public float beatStart = 3f;
        bool running = false;
        bool hasDRankTransition = false;
        bool playedFirstTransition = false;
        bool hasSRankTransition = false;
        bool hasD;
        bool hasC;
        bool hasB;
        bool hasA;
        bool hasS;
        bool hasSS;
        bool hasSSS;
        public void Start()
        {
            spawner = GetComponent<WaveSpawner>();
            spawner.OnWaveBeginEvent.AddListener(Replace);
            spawner.OnWaveAnyEndEvent.AddListener(Return);
            foreach (string address in strings)
            {
                Catalog.LoadAssetAsync<AudioContainer>(address, value =>
                {
                    if (!audioContainers.ContainsKey(address))
                        audioContainers.Add(address, value);
                }, "Devil Trigger");
            }
            audioSources.Add(spawner.gameObject.AddComponent<AudioSource>());
            audioSources.Add(spawner.gameObject.AddComponent<AudioSource>());
            step = spawner.gameObject.AddComponent<AudioSource>();
            announcer = spawner.gameObject.AddComponent<AudioSource>();
            audioSources[0].outputAudioMixerGroup = ThunderRoadSettings.GetAudioMixerGroup(AudioMixerName.Music);
            audioSources[1].outputAudioMixerGroup = ThunderRoadSettings.GetAudioMixerGroup(AudioMixerName.Music);
            step.outputAudioMixerGroup = ThunderRoadSettings.GetAudioMixerGroup(AudioMixerName.Music);
            announcer.outputAudioMixerGroup = ThunderRoadSettings.GetAudioMixerGroup(AudioMixerName.UI);
            step.loop = false;
            announcer.loop = false;
            nextEventTime = AudioSettings.dspTime - beatStart;
        }
        public void Update()
        {
            if (!running)
            {
                return;
            }
            else
            {
                double time = AudioSettings.dspTime;
                if (time > nextEventTime && MusicModule.score < MusicModule.DRankScore)
                {
                    if (hasDRankTransition)
                    {
                        index = 0;
                        hasSRankTransition = false;
                        hasDRankTransition = false;
                    }
                    audioSources[flip].clip = audioContainers["Jenix.DTOpening"].PickAudioClip(index);
                    audioSources[flip].PlayScheduled(nextEventTime);
                    nextEventTime += audioContainers["Jenix.DTOpening"].PickAudioClip(index).length - 3;
                    flip = 1 - flip;
                    hasDRankTransition = false;
                    playedFirstTransition = false;
                }
                else if (MusicModule.score >= MusicModule.DRankScore && MusicModule.score < MusicModule.SRankScore && !hasDRankTransition)
                {
                    if (!playedFirstTransition)
                    {
                        index = 0;
                        playedFirstTransition = true;
                        audioSources[flip].clip = audioContainers["Jenix.DTDRankTransition"].PickAudioClip(0);
                        nextEventTime = AudioSettings.dspTime;
                        audioSources[flip].PlayScheduled(nextEventTime);
                        nextEventTime += audioContainers["Jenix.DTDRankTransition"].PickAudioClip(0).length - 3;
                        flip = 1 - flip;
                    }
                    else
                    {
                        audioSources[flip].clip = audioContainers["Jenix.DTDRankTransition"].PickAudioClip(1);
                        audioSources[flip].PlayScheduled(nextEventTime);
                        nextEventTime += audioContainers["Jenix.DTDRankTransition"].PickAudioClip(1).length - 3;
                        flip = 1 - flip;
                        hasDRankTransition = true;
                        playedFirstTransition = false;
                    }
                }
                else if (time + 1.5 > nextEventTime && MusicModule.score < MusicModule.SRankScore && hasDRankTransition)
                {
                    if (hasSRankTransition)
                    {
                        index = 0;
                        hasSRankTransition = false;
                    }
                    audioSources[flip].clip = audioContainers["Jenix.DTDRank"].PickAudioClip(index);
                    audioSources[flip].PlayScheduled(nextEventTime - 1.5);
                    if (index == 0 || index == 4)
                    {
                        nextEventTime += audioContainers["Jenix.DTDRank"].PickAudioClip(index).length - 4.5;
                    }
                    else
                    nextEventTime += audioContainers["Jenix.DTDRank"].PickAudioClip(index).length - 6;
                    flip = 1 - flip;
                    index++;
                    if (index >= audioContainers["Jenix.DTDRank"].sounds.Count) index = 0;
                    playedFirstTransition = false;
                    hasSRankTransition = false;
                }
                else if (MusicModule.score >= MusicModule.SRankScore && !hasSRankTransition)
                {
                    index = 0;
                    audioSources[flip].clip = audioContainers["Jenix.DTSRankTransition"].PickAudioClip(index);
                    nextEventTime = AudioSettings.dspTime;
                    audioSources[flip].PlayScheduled(nextEventTime);
                    nextEventTime += audioContainers["Jenix.DTSRankTransition"].PickAudioClip(index).length - 1.5;
                    flip = 1 - flip;
                    hasSRankTransition = true;
                }
                else if (time + 1.5 > nextEventTime && MusicModule.score >= MusicModule.SRankScore && hasSRankTransition)
                {
                    audioSources[flip].clip = audioContainers["Jenix.DTSRank"].PickAudioClip(index);
                    audioSources[flip].PlayScheduled(nextEventTime - 1.5);
                    nextEventTime += audioContainers["Jenix.DTSRank"].PickAudioClip(index).length - 6;
                    flip = 1 - flip;
                    index++;
                    if (index >= audioContainers["Jenix.DTSRank"].sounds.Count) index = 0;
                }
                if (MusicModule.Announcer)
                    if (MusicModule.score <= MusicModule.DRankScore)
                    {
                        hasD = false;
                        hasC = false;
                        hasB = false;
                        hasA = false;
                        hasS = false;
                        hasSS = false;
                        hasSSS = false;
                    }
                    else if (MusicModule.score >= MusicModule.DRankScore && MusicModule.score < MusicModule.CRankScore && (!hasD || hasC))
                    {
                        hasD = true;
                        hasC = false;
                        hasB = false;
                        hasA = false;
                        hasS = false;
                        hasSS = false;
                        hasSSS = false;
                        announcer.clip = audioContainers["Jenix.DAnnouncer"].GetRandomAudioClip();
                        announcer.Play();
                    }
                    else if (MusicModule.score >= MusicModule.CRankScore && MusicModule.score < MusicModule.BRankScore && (!hasC || hasB))
                    {
                        hasD = true;
                        hasC = true;
                        hasB = false;
                        hasA = false;
                        hasS = false;
                        hasSS = false;
                        hasSSS = false;
                        announcer.clip = audioContainers["Jenix.CAnnouncer"].GetRandomAudioClip();
                        announcer.Play();
                    }
                    else if (MusicModule.score >= MusicModule.BRankScore && MusicModule.score < MusicModule.ARankScore && (!hasB || hasA))
                    {
                        hasD = true;
                        hasC = true;
                        hasB = true;
                        hasA = false;
                        hasS = false;
                        hasSS = false;
                        hasSSS = false;
                        announcer.clip = audioContainers["Jenix.BAnnouncer"].GetRandomAudioClip();
                        announcer.Play();
                    }
                    else if (MusicModule.score >= MusicModule.ARankScore && MusicModule.score < MusicModule.SRankScore && (!hasA || hasS))
                    {
                        hasD = true;
                        hasC = true;
                        hasB = true;
                        hasA = true;
                        hasS = false;
                        hasSS = false;
                        hasSSS = false;
                        announcer.clip = audioContainers["Jenix.AAnnouncer"].GetRandomAudioClip();
                        announcer.Play();
                    }
                    else if (MusicModule.score >= MusicModule.SRankScore && MusicModule.score < MusicModule.SSRankScore && (!hasS || hasSS))
                    {
                        hasD = true;
                        hasC = true;
                        hasB = true;
                        hasA = true;
                        hasS = true;
                        hasSS = false;
                        hasSSS = false;
                        announcer.clip = audioContainers["Jenix.SAnnouncer"].GetRandomAudioClip();
                        announcer.Play();
                    }
                    else if (MusicModule.score >= MusicModule.SSRankScore && MusicModule.score < MusicModule.SSSRankScore && (!hasSS || hasSSS))
                    {
                        hasD = true;
                        hasC = true;
                        hasB = true;
                        hasA = true;
                        hasS = true;
                        hasSS = true;
                        hasSSS = false;
                        announcer.clip = audioContainers["Jenix.SSAnnouncer"].GetRandomAudioClip();
                        announcer.Play();
                    }
                    else if (MusicModule.score >= MusicModule.SSSRankScore && !hasSSS)
                    {
                        hasD = true;
                        hasC = true;
                        hasB = true;
                        hasA = true;
                        hasS = true;
                        hasSS = true;
                        hasSSS = true;
                        announcer.clip = audioContainers["Jenix.SSSAnnouncer"].GetRandomAudioClip();
                        announcer.Play();
                    }
            }
        }
        public void Replace()
        {
            if (MusicModule.EnableMusic)
            {
                audioSources[1 - flip].clip = null;
                audioSources[1 - flip].Stop();
                audioSources[flip].clip = audioContainers["Jenix.DTOpening"].PickAudioClip(0);
                nextEventTime = AudioSettings.dspTime;
                audioSources[flip].PlayScheduled(nextEventTime);
                nextEventTime += audioContainers["Jenix.DTOpening"].PickAudioClip(0).length - 6;
                flip = 1 - flip;
                running = true;
                MusicModule.score = 0;
                ThunderBehaviourSingleton<MusicManager>.Instance.Volume = 0;
            }
        }
        public void Return()
        {
            if (running)
            {
                step.clip = audioContainers["Jenix.DTEnding"].PickAudioClip(0);
                nextEventTime = AudioSettings.dspTime;
                step.PlayScheduled(nextEventTime);
                Level.current.StartCoroutine(Utils.FadeOut(audioSources[1 - flip], 3f));
                flip = 1 - flip;
                index = 0;
                running = false;
                MusicModule.score = 0;
                hasDRankTransition = false;
                hasSRankTransition = false;
                playedFirstTransition = false;
                hasD = false;
                hasC = false;
                hasB = false;
                hasA = false;
                hasS = false;
                hasSS = false;
                hasSSS = false;
                ThunderBehaviourSingleton<MusicManager>.Instance.Volume = 1;
            }
        }
    }
}