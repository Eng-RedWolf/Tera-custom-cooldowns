﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TCC.Parsing;

namespace TCC
{
    /// <summary>
    /// Logica di interazione per BossGage.xaml
    /// </summary>
    public partial class BossGage : UserControl
    {
        NumberFormatInfo nfi = new NumberFormatInfo { NumberGroupSeparator = "`", NumberDecimalDigits = 0 };

        public ulong EntityId
        {
            get { return (ulong)GetValue(EntityIdProperty); }
            set { SetValue(EntityIdProperty, value); }
        }
        public static readonly DependencyProperty EntityIdProperty = DependencyProperty.Register("EntityId", typeof(ulong), typeof(BossGage));

        public float MaxHP
        {
            get { return (float)GetValue(MaxHPProperty); }
            set { SetValue(MaxHPProperty, value); }
        }
        public static readonly DependencyProperty MaxHPProperty = DependencyProperty.Register("MaxHP", typeof(float), typeof(BossGage));

        public string BossName
        {
            get { return (string)GetValue(BossNameProperty); }
            set { SetValue(BossNameProperty, value); }
        }
        public static readonly DependencyProperty BossNameProperty = DependencyProperty.Register("BossName", typeof(string), typeof(BossGage));

        bool enraged;
        public bool Enraged
        {
            get { return enraged; }
            set
            {
                if (value != Enraged)
                {
                    if (!value)
                    {
                        LastEnragePercent = 100 * CurrentHP / MaxHP;
                        Console.WriteLine("Last enrage percentage = {0} ({1}/{2})", LastEnragePercent, CurrentHP, MaxHP);
                    }
                    enraged = value;
                }
            }
        }

        float currentHP;
        public float CurrentHP
        {
            get { return currentHP; }
            set
            {
                currentHP = value;
            }
        }





        int AnimationTime = 150;
        float LastEnragePercent = 100;
        int EnrageDuration = 36000;
        int CurrentEnrageTime = 36;

        Timer NumberTimer = new Timer(1000);

        public BossGage()
        {
            InitializeComponent();

            PacketRouter.EnragedChanged += BossGage_EnragedUpdated;
            PacketRouter.BossHPChanged += BossGage_HPUpdated;
            BossNameTB.DataContext = this;

        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            HPrect.Fill = new SolidColorBrush(Colors.OrangeRed);
        }

        private void BossGage_HPUpdated(ulong id, object hp)
        {
            float val = (float)hp;

            Dispatcher.Invoke(() =>
            {
                if (id == EntityId)
                {
                    HPrect.BeginAnimation(WidthProperty, GetDoubleAnimation(ValueToLength(val, MaxHP)));
                    Perc.Text = String.Format("{0:0.0}%", 100 * val / MaxHP);
                    Perc2.Text = String.Format("{0} / {1}", val.ToString("n", nfi), MaxHP.ToString("n", nfi));
                    if (Enraged)
                    {
                        NextEnrage.BeginAnimation(MarginProperty, SlideNextEnrageIndicatorDirectAnimation(HPrect.ActualWidth - 2));
                    }
                    CurrentHP = val;
                }
            });
        }

        private void BossGage_EnragedUpdated(ulong id, object enraged)
        {
            Dispatcher.Invoke(() =>
            {
                Enraged = (bool)enraged;

                if (id == this.EntityId)
                {
                    if ((bool)enraged)
                    {
                        Enraged = (bool)enraged;
                        number.Text = CurrentEnrageTime.ToString();
                        NextEnrage.BeginAnimation(MarginProperty, SlideNextEnrageIndicatorDirectAnimation(HPrect.ActualWidth - 2));
                        HPrect.Effect.BeginAnimation(DropShadowEffect.OpacityProperty, GetDoubleAnimation(1));
                        HPrect.Fill.BeginAnimation(SolidColorBrush.ColorProperty, GetColorAnimation(Colors.Red));
                        EnrageArc.BeginAnimation(Arc.EndAngleProperty, new DoubleAnimation(359.9, 0, TimeSpan.FromMilliseconds(EnrageDuration)));
                        EnrageGrid.BeginAnimation(WidthProperty, GetDoubleAnimation(60));
                        NumberTimer = new Timer(1000);
                        NumberTimer.Elapsed += (s, ev) =>
                        {
                            Dispatcher.Invoke(() =>
                            {
                                number.Text = CurrentEnrageTime.ToString();
                                CurrentEnrageTime--;
                            });
                        };
                        NumberTimer.Enabled = true;
                    }
                    else
                    {
                        if (NumberTimer != null)
                        {
                            NumberTimer.Stop();
                        }
                        NextEnrage.BeginAnimation(MarginProperty, SlideNextEnrageIndicatorAnimation(LastEnragePercent - 10));
                        HPrect.Effect.BeginAnimation(DropShadowEffect.OpacityProperty, GetDoubleAnimation(0));
                        HPrect.Fill.BeginAnimation(SolidColorBrush.ColorProperty, GetColorAnimation(Color.FromArgb(0xff,0x4a,0x82,0xbd)));
                        EnrageGrid.BeginAnimation(WidthProperty, GetDoubleAnimation(0));
                        CurrentEnrageTime = 36;
                    }

                }
            });
        }
        double ValueToLength(double value, double maxValue)
        {
            if (maxValue == 0)
            {
                return 0;
            }
            else
            {
                double n = BaseRect.Width * ((double)value / (double)maxValue);
                return n;
            }

        }
        private DoubleAnimation GetDoubleAnimation(double value)
        {
            return new DoubleAnimation(value, TimeSpan.FromMilliseconds(AnimationTime)) { EasingFunction = new QuadraticEase() };
        }
        private ColorAnimation GetColorAnimation(Color c)
        {
            return new ColorAnimation(c, TimeSpan.FromMilliseconds(AnimationTime)) { EasingFunction = new QuadraticEase() };
        }

        private ThicknessAnimation SlideNextEnrageIndicatorAnimation(double percentage)
        {
            var t = percentage * 398 / 100;
            //Console.WriteLine("New position: {0}", t);
            return new ThicknessAnimation(new Thickness(t, 0, 0, 0), TimeSpan.FromSeconds(.4)) { EasingFunction = new QuadraticEase() };
        }
        private ThicknessAnimation SlideNextEnrageIndicatorDirectAnimation(double thickness)
        {
            Console.WriteLine("New direct position: {0}", thickness);
            return new ThicknessAnimation(new Thickness(thickness, 0, 0, 0), TimeSpan.FromSeconds(.4)) { EasingFunction = new QuadraticEase() };

        }

    }

}