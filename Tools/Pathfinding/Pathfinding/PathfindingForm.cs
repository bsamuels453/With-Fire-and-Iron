using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pathfinding {
    public partial class PathfindingForm : Form {
        public PathfindingForm() {
            InitializeComponent();
        }

        const float _timeDelta = 100f;//in ms
        const float _timeMultiplier = 30f;//ms between each timedelta
        const float _span = 5000000;

        private void StartSimButton_Click(object sender, EventArgs e) {
            var task = new Task(Invoke);
            task.Start();
            allowTick = true;

            var f1 = (float)Math.Atan2(0, 1);
            var f2 = (float)Math.Atan2(1, 0);
            var f3 = (float)Math.Atan2(0, -1);
            var f4 = (float)Math.Atan2(-1, 0);

            int g = 5;
        }

        void Invoke(){
            var pathfinder = new Pathfinder(_timeDelta);
            var sw = new Stopwatch();
            for (float i = 0; i < _span; i += _timeDelta) {
                sw.Start();
                float vel, turn, angleDiff, curAngle;
                var ret = pathfinder.Tick(out vel, out turn, out angleDiff, out curAngle);
                Action t = () => SetPicture(ret, vel, turn, angleDiff, curAngle);
                try {
                    this.Invoke(t);
                }
                catch{
                }
                while (sw.ElapsedMilliseconds < _timeMultiplier || !allowTick) {
                    Thread.Sleep(1);
                }
                sw.Reset();
            }

        }

        public void SetPicture(Bitmap bmp, float vel, float turn, float angleDiff, float curAngle){
            pictureBox1.Image = bmp;
            VelocityVal.Text = vel.ToString();
            TurnRateVal.Text = turn.ToString();
            AngleDiffField.Text = angleDiff.ToString();
            CurAngleText.Text = curAngle.ToString();
        }

        private void pictureBox1_Click(object sender, EventArgs e) {

        }

        private void Form1_Load(object sender, EventArgs e) {

        }

        private void VelocityVal_Click(object sender, EventArgs e) {

        }

        private void label4_Click(object sender, EventArgs e) {

        }


        bool allowTick;
        private void button1_Click(object sender, EventArgs e) {
            allowTick = !allowTick;
        }
    }
}
