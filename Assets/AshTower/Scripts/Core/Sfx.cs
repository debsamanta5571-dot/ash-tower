using UnityEngine;

namespace AshTower
{
    public static class Sfx
    {
        const string PrefKey = "ash_volume";
        static AudioSource _src;
        static AudioClip _ui, _play, _hit, _block, _draw, _death, _win, _energy, _hover;

        public static float Volume => PlayerPrefs.GetFloat(PrefKey, 0.5f);

        public static void SetVolume(float v)
        {
            v = Mathf.Clamp01(v);
            PlayerPrefs.SetFloat(PrefKey, v);
            PlayerPrefs.Save();
            ApplyVolume();
        }

        static void ApplyVolume()
        {
            if (_src != null) _src.volume = Volume;
        }

        public static void Init(Transform parent)
        {
            var go = new GameObject("Sfx");
            go.transform.SetParent(parent, false);
            _src = go.AddComponent<AudioSource>();
            _src.playOnAwake = false;
            ApplyVolume();
            _ui = Tone(880, 0.06f, 0.18f, 0);
            _hover = Tone(1320, 0.04f, 0.08f, 0);
            _play = Noise(0.12f, 0.22f, 420, 0.4f);
            _hit = Noise(0.18f, 0.35f, 90, 0.7f);
            _block = Tone(220, 0.14f, 0.25f, 1);
            _draw = Tone(640, 0.07f, 0.12f, 0);
            _death = Noise(0.4f, 0.4f, 55, 1f);
            _win = Arp(new[] { 523f, 659f, 784f, 1046f }, 0.12f);
            _energy = Tone(520, 0.09f, 0.16f, 0);
        }

        public static void Ui() => Play(_ui);
        public static void Hover() => Play(_hover, 0.6f);
        public static void PlayCard() => Play(_play);
        public static void Hit() => Play(_hit);
        public static void Block() => Play(_block);
        public static void Draw() => Play(_draw, 0.7f);
        public static void Death() => Play(_death);
        public static void Win() => Play(_win);
        public static void Energy() => Play(_energy);

        static void Play(AudioClip c, float v = 1f)
        {
            if (_src == null || c == null) return;
            _src.PlayOneShot(c, v);
        }

        static AudioClip Tone(float freq, float dur, float vol, int shape)
        {
            int sr = 22050;
            int n = Mathf.Max(8, (int)(sr * dur));
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sr;
                float env = Mathf.Pow(1f - (float)i / n, 1.4f);
                float ph = 2f * Mathf.PI * freq * t;
                float s = shape == 0 ? Mathf.Sin(ph) : (Mathf.Sin(ph) > 0 ? 0.5f : -0.5f);
                data[i] = s * env * vol;
            }
            var clip = AudioClip.Create("t", n, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip Noise(float dur, float vol, float hp, float crunch)
        {
            int sr = 22050;
            int n = Mathf.Max(8, (int)(sr * dur));
            var data = new float[n];
            float acc = 0;
            for (int i = 0; i < n; i++)
            {
                float env = Mathf.Pow(1f - (float)i / n, 1.8f);
                acc = acc * 0.6f + (Random.value * 2f - 1f);
                float s = Mathf.Lerp(Random.value * 2f - 1f, acc, crunch);
                data[i] = s * env * vol;
            }
            var clip = AudioClip.Create("n", n, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip Arp(float[] notes, float step)
        {
            int sr = 22050;
            int sn = (int)(sr * step);
            int n = sn * notes.Length;
            var data = new float[n];
            for (int k = 0; k < notes.Length; k++)
                for (int i = 0; i < sn; i++)
                {
                    float env = Mathf.Pow(1f - (float)i / sn, 1.2f);
                    data[k * sn + i] = Mathf.Sin(2f * Mathf.PI * notes[k] * i / sr) * env * 0.22f;
                }
            var clip = AudioClip.Create("a", n, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
