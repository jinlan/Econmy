using System;
using System.Collections.Generic;
using Verse;

namespace RimEconomy {
    public class ExposableList<T> : List<T>, IExposable {
        public Action<List<T>> Exposer;
        void IExposable.ExposeData() {
            if(this.Exposer != null) {
                this.Exposer(this);
            }
        }
    }
}
