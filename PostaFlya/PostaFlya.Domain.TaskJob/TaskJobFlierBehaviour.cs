using System;
using System.Collections.Generic;
using System.Linq;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Location;
using WebSite.Infrastructure.Domain;

namespace PostaFlya.Domain.TaskJob
{
    [Serializable]
    public class TaskJobFlierBehaviour : FlierBehaviourBase<TaskJobFlierBehaviourInterface>, TaskJobFlierBehaviourInterface
    { 
        public Locations ExtraLocations { get; set; }
        public double MaxAmount { get; set; }

        #region FlierProperties
        public double CostOverhead
        {
            get { return _flierProperties.CostOverhead; }
            set { _flierProperties.CostOverhead = value; }
        }


        private Flier.Flier _flier;
        public override Flier.Flier Flier
        {
            get { return _flier; }
            set { 
                _flier = value;
                UpdateFlierProperties();
            }
        }

        private TaskJobFlierBehaviourFlierProperties _flierProperties = new TaskJobFlierBehaviourFlierProperties(new Dictionary<string, object>());
        public override Dictionary<string, object> FlierProperties
        {
            get { return _flierProperties != null ? _flierProperties.PropertyGroup : null; }
            set { 
                _flierProperties = value != null ? new TaskJobFlierBehaviourFlierProperties(value) : null;
                UpdateFlierProperties();
            }
        }
        #endregion

        private void UpdateFlierProperties()
        {
            if (Flier == null || _flierProperties == null) return;
            if (Flier.ExtendedProperties == null)
                Flier.ExtendedProperties = _flierProperties.PropertyGroup;
            else
            {
                foreach (var kv in _flierProperties.PropertyGroup.ToArray())
                {
                    Flier.ExtendedProperties[kv.Key] = kv.Value;
                }
                _flierProperties = new TaskJobFlierBehaviourFlierProperties(Flier.ExtendedProperties);
            }

        }

    }

    [Serializable]
    public class TaskJobFlierBehaviourFlierProperties
    {
        private readonly Dictionary<string, object> _source;

        public TaskJobFlierBehaviourFlierProperties(Dictionary<string, object> source)
        {
            if (source == null)
                source = new Dictionary<string, object>();
            _source = source;
        }

        public double CostOverhead
        {
            get { return (double)(_source["CostOverhead"] ?? 0.0); }
            set { _source["CostOverhead"] = value; }
        }

        public Dictionary<string, object> PropertyGroup
        {
            get { return _source; }
        }

    }
}