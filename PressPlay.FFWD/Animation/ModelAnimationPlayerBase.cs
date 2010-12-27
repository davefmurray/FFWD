#region File Description
//-----------------------------------------------------------------------------
// ModelAnimationPlayerBase.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PressPlay.FFWD.Components
{
    /// <summary>
    /// This class serves as a base class for various animation players.  It contains
    /// common functionality to deal with a clip, playing it back at a speed, 
    /// notifying clients of completion, etc.
    /// </summary>
    public abstract class ModelAnimationPlayerBase
    {
        // Clip currently being played
        AnimationClip currentClipValue;

        // State of the current animation
        AnimationState currentState;

        // Current timeindex and keyframe in the clip
        int currentKeyframe;

        /// <summary>
        /// Gets the clip currently being decoded.
        /// </summary>
        public AnimationClip CurrentClip
        {
            get { return currentClipValue; }
        }

        /// <summary>
        /// Get/Set the current key frame index
        /// </summary>
        public int CurrentKeyFrame
        {
            get { return currentKeyframe; }
            set
            {
                IList<ModelKeyframe> keyframes = currentClipValue.Keyframes;
                TimeSpan time = keyframes[value].Time;
                CurrentTimeValue = time;
            }
        }

        private TimeSpan currentTimeValue;
        /// <summary>
        /// Gets/set the current play position.
        /// </summary>
        public TimeSpan CurrentTimeValue
        {
            get { return currentTimeValue; }
            set
            {
                TimeSpan time = value;

                // If the position moved backwards, reset the keyframe index.
                if (time < currentTimeValue)
                {
                    currentKeyframe = 0;
                    InitClip();
                }

                currentTimeValue = time;

                // Read keyframe matrices.
                IList<ModelKeyframe> keyframes = currentClipValue.Keyframes;

                while (currentKeyframe < keyframes.Count)
                {
                    ModelKeyframe keyframe = keyframes[currentKeyframe];

                    // Stop when we've read up to the current time position.
                    if (keyframe.Time > currentTimeValue)
                        break;

                    // Use this keyframe
                    SetKeyframe(keyframe);

                    currentKeyframe++;
                }
            }
        }

        /// <summary>
        /// Starts playing a clip
        /// </summary>
        /// <param name="clip">Animation clip to play</param>
        /// <param name="playbackRate">Speed to playback</param>
        /// <param name="duration">Length of time to play (max is looping, 0 is once)</param>
        public void StartClip(AnimationClip clip, AnimationState state)
        {
            if (clip == null)
                throw new ArgumentNullException("Clip required");

            // Store the clip and reset playing data            
            currentClipValue = clip;
            currentKeyframe = Math.Max(0, state.firstFrame);
            CurrentTimeValue = clip.Keyframes[currentKeyframe].Time;
            currentState = state;
            currentState.time = (float)CurrentTimeValue.TotalSeconds;
            currentState.length = (float)clip.Keyframes[Math.Min(state.lastFrame, clip.Keyframes.Count - 1)].Time.TotalSeconds; 
            currentState.enabled = true;

            // Call the virtual to allow initialization of the clip
            InitClip();
        }

        /// <summary>
        /// Will pause the playback of the current clip
        /// </summary>
        public void PauseClip()
        {
            currentState.enabled = false;
        }

        /// <summary>
        /// Will resume playback of the current clip
        /// </summary>
        public void ResumeClip()
        {
            currentState.enabled = true;
        }

        /// <summary>
        /// Virtual method allowing subclasses to do any initialization of data when the clip is initialized.
        /// </summary>
        protected virtual void InitClip()
        {
        }

        /// <summary>
        /// Virtual method allowing subclasses to set any data associated with a particular keyframe.
        /// </summary>
        /// <param name="keyframe">Keyframe being set</param>
        protected virtual void SetKeyframe(ModelKeyframe keyframe)
        {
        }

        /// <summary>
        /// Virtual method allowing subclasses to perform data needed after the animation 
        /// has been updated for a new time index.
        /// </summary>
        protected virtual void OnUpdate()
        {
        }

        /// <summary>
        /// Called during the update loop to move the animation forward
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update()
        {
            if (currentClipValue == null)
                return;

            if (!currentState.enabled)
                return;

            float time = Time.deltaTime;

            // Adjust for the rate
            if (currentState.speed != 1.0f)
                time = time * currentState.speed;

            currentState.time += time;

            // See if we should terminate
            if (currentState.time > currentState.length)
            {
                if (currentState.wrapMode == WrapMode.Once)
                {
                    currentState.enabled = false;
                    return;
                }
                if (currentState.wrapMode == WrapMode.Loop)
                {
                    currentState.time -= currentState.length;
                }
                if (currentState.wrapMode == WrapMode.PingPong)
                {
                    currentState.speed *= -1;
                }
                if (currentState.wrapMode == WrapMode.Default)
                {
                    throw new NotImplementedException("What to do here?");
                }
            }

            CurrentTimeValue = TimeSpan.FromSeconds(currentState.time);

            OnUpdate();
        }
    }
}
