using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackCoat;
using BlackCoat.ParticleSystem;

namespace BlackTournament.Particles
{
    class TriggerComposite : CompositeEmitter
    {
        public TriggerComposite(Core core, IEnumerable<BaseEmitter> emitters = null) : base(core, emitters)
        {
        }

        public void Trigger()
        {
            foreach (var emitter in _Emitters)
            {
                switch (emitter)
                {
                    case SmokeEmitter smoke: // TODO: replace with interface
                        smoke.Trigger();
                    break;
                    case SparkEmitter spark:
                        spark.Trigger();
                    break;
                    case WaveEmitter wave:
                        wave.Trigger();
                    break;
                }
            }
        }
    }
}