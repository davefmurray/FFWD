﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using PressPlay.FFWD.Interfaces;
using PressPlay.FFWD;
using Microsoft.Xna.Framework;

namespace PressPlay.FFWD.Components
{
    public class Animation : Behaviour, IInitializable, IEnumerable<AnimationState>
	{
        [ContentSerializer(ElementName = "clip", Optional=true)]
        private int clipId = 0;
        public bool playAutomatically;
        public WrapMode wrapMode;
        [ContentSerializer(ElementName = "clips")]
        private int[] clipsId = null;

        private string defaultClip;
        [ContentSerializerIgnore]
		public AnimationClip clip
        {
            get
            {
                return GetClip(defaultClip);
            }
            set
            {
                AddClip(value, value.name);
                defaultClip = value.name;
            }
        }

		private Dictionary<string, int> stateIndexes = new Dictionary<string, int>();
        private List<AnimationState> states;

        public void Initialize(AssetHelper assets)
        {
            if (clipsId != null)
            {
                states = new List<AnimationState>(clipsId.Length);
                for (int i = 0; i < clipsId.Length; i++)
                {
                    AnimationClip data = assets.LoadAsset<AnimationClip>(clipsId[i]);
                    if (data != null)
                    {
                        AddClip(data, data.name);
                        if (clipsId[i] == clipId)
                        {
                            defaultClip = data.name;
                        }
                    }
                }
            }
        }

        public override void Awake()
        {
            base.Awake();
            if (playAutomatically && clip != null)
            {
                Play(defaultClip);
            }
        }

		public AnimationState this[string index]
		{
			get
			{
				if (stateIndexes.ContainsKey(index))
				{
					return states[stateIndexes[index]];
				}
				return null;
			}
		}

		public AnimationClip GetClip(string name)
		{
            if (stateIndexes.ContainsKey(name))
            {
                return states[stateIndexes[name]].clip;
            }
            return null;
		}

		public bool isPlaying
        {
			get 
            {
                return states.Any(s => s.enabled);
            } 
		}

		public void Rewind()
		{
			// TODO : Add implementation of method
			throw new NotImplementedException("Method not implemented.");
		}

		public bool Play()
		{
			return Play(defaultClip);
		}

		public bool Play(string name)
		{
            if (String.IsNullOrEmpty(name))
            {
                return false;
            }            
            if (!stateIndexes.ContainsKey(name))
            {
                return false;
            }
            this[name].enabled = true;
            return true;
        }

		public void PlayQueued(string name)
		{
			PlayQueued(name, QueueMode.CompleteOthers);
		}

		public void PlayQueued(string name, QueueMode mode)
		{
			// TODO : Add implementation of method
            throw new NotImplementedException("Method not implemented.");
		}

		public void Stop()
		{
            for (int i = 0; i < states.Count; i++)
            {
                states[i].enabled = false;
            }
		}

		public void Stop(string name)
		{
			if (stateIndexes.ContainsKey(name))
			{
				states[stateIndexes[name]].enabled = false;
			}
		}

		public void AddClip(AnimationClip clip, string newName)
		{
			if (String.IsNullOrEmpty(newName))
			{
				return;
			}
            if (states == null)
            {
                states = new List<AnimationState>(1);
            }
            if (stateIndexes.ContainsKey(newName))
            {
                states[stateIndexes[newName]] = new AnimationState(this, clip);
            }
            else
            {
                stateIndexes[newName] = states.Count;
                states.Add(new AnimationState(this, clip));
            }
		}

		public void AddClip(AnimationClip clip, string newName, int firstFrame, int lastFrame)
		{
			AnimationClip newClip = new AnimationClip(clip, newName, firstFrame, lastFrame);
			AddClip(newClip, newName);
		}

		public void AddClip(AnimationClip clip, string newName, int firstFrame, int lastFrame, bool addLoopFrame)
		{
			AddClip(clip, newName, firstFrame, lastFrame);
			// TODO: Add the loop frame - which is a frame identical to the first frame at the end. We need to figure out how to do that properly.
		}

		public void Blend(string name)
		{
			Blend(name, 1.0f, 1.0f);
		}

		public void Blend(string name, float weight)
		{
			Blend(name, weight, 1.0f);
		}

		public void Blend(string name, float weight, float length)
		{
			// TODO : Add implementation of method
			//throw new NotImplementedException("Method not implemented.");
            //Rewind();
            Stop();
            Play(name);
		}

		public void CrossFade(string name)
		{
			CrossFade(name, 0.3f);
		}

		public void CrossFade(string name, float fadeLength)
		{
            //Rewind();
            Stop();
			Play(name);
		}

		public void Sample()
		{
		}

        public bool IsPlaying(string name)
        {
            if (stateIndexes.ContainsKey(name))
            {
                return states[stateIndexes[name]].enabled;
            }
            return false;
        }

        public int GetClipCount()
        {
            return states.Count;
        }

        public IEnumerator<AnimationState> GetEnumerator()
        {
            return states.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return stateIndexes.Values.GetEnumerator();
        }

        internal void UpdateAnimationStates(float deltaTime)
        {
            for (int i = 0; i < states.Count; i++)
            {
                states[i].Update(deltaTime);
            }
        }
    }
}
