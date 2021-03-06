using UnityEngine;
using UnityEngine.Events;

namespace Fragsurf.Utility
{
	public class TimeStep : SingletonComponent<TimeStep>
	{
		// Elapsed Time, Delta Time
		public UnityEvent<float, float> OnTick = new UnityEvent<float, float>();
		// Elapsed Time, Delta Time, Alpha
		public UnityEvent<float, float, float> OnFrame = new UnityEvent<float, float, float>();

		private float _accumulator;
		private float _desiredTimeScale = 1f;

		public FPSCounter FPSCounter { get; } = new FPSCounter();
		public float ElapsedTime { get; private set; }
		public float DeltaTime { get; private set; }
		public float FixedDeltaTime { get; private set; }
		public float Alpha { get; private set; }

		[ConVar("game.targetfps", "Maximum frames to render per second", ConVarFlags.UserSetting)]
		public int TargetFPS
		{
			get => Application.targetFrameRate;
			set => Application.targetFrameRate = Mathf.Clamp(value, 10, 2000);
		}

		[ConVar("game.tickrate", "Ticks to process per second", ConVarFlags.Replicator)]
		public int TickRate
		{
			get => (int)(1f / Time.fixedDeltaTime);
			set => Time.fixedDeltaTime = 1f / Mathf.Clamp(value, 20, 512);
		}

		[ConVar("game.timescale", "", ConVarFlags.Replicator)]
		public float TimeScale
		{
			get => Time.timeScale;
			set => Time.timeScale = Mathf.Clamp(value, 0.1f, 10f);
		}

		private void Awake()
        {
			DevConsole.RegisterObject(this);
        }

        protected override void OnDestroy()
        {
			base.OnDestroy();

			DevConsole.RemoveAll(this);
        }

		// This is the magic I had in previous FSGameLoop time-stepper
		// Saving it incase any of it actually had purpose...
		//    Time.fixedDeltaTime = 1f / TickRate;
		//    var timeScaleModifier = FPSCounter.AverageFPS > 0 && FPSCounter.AverageFPS < TickRate ? (float)TickRate / FPSCounter.AverageFPS : 1f;

		//    Time.timeScale = _desiredTimeScale * timeScaleModifier;
		//    var dt = Time.fixedDeltaTime * (1f / Time.timeScale);
		//    var newTime = Time.realtimeSinceStartup;
		//    var frameTime = newTime - ElapsedTime;
		//    if (frameTime > Time.fixedDeltaTime)
		//        frameTime = Time.fixedDeltaTime;
		//    // todo: check timescale is accurately placed.
		//    // idk if its necessary to apply it to maximumDeltaTime..
		//    Time.maximumDeltaTime = Time.fixedDeltaTime * (1f / Time.timeScale);
		//    ElapsedTime = newTime;
		//    _accumulator += frameTime;

		void Update()
		{
			DeltaTime = Time.deltaTime;
			FixedDeltaTime = (1f / TickRate) * (1f / Time.timeScale);

			var newTime = Time.realtimeSinceStartup;
			var frameTime = Mathf.Min(newTime - ElapsedTime, FixedDeltaTime);

			ElapsedTime = newTime;
			_accumulator += frameTime;

			while (_accumulator >= FixedDeltaTime)
			{
				_accumulator -= FixedDeltaTime;
				OnTick?.Invoke(ElapsedTime, FixedDeltaTime);
			}
			Alpha = _accumulator / FixedDeltaTime;

			OnFrame?.Invoke(ElapsedTime, DeltaTime, Alpha);

			FPSCounter.Update();
		}

	}
}

