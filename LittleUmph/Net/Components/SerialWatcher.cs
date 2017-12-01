using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LittleUmph.Net.Components
{
    public partial class SerialWatcher : Component
    {
        public SerialWatcher()
        {
            InitializeComponent();
        }

        public SerialWatcher(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
    }
}
