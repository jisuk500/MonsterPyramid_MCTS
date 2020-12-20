using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MonsterPyramid.Model;
using MonsterPyramid.ViewModel.MainWindow;

namespace MonsterPyramid.View.Main
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 다른 땅 클릭시 리스트뷰 셀렉션이 초기화되도록 하는데 쓰이는 전역변수
        /// </summary>
        private bool wasListviewClicked = false;
        /// <summary>
        /// 이 창의 View Model
        /// </summary>
        private MainWindowViewModel VM { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            VM = new MainWindowViewModel();
            this.DataContext = VM;

        }

        /// <summary>
        /// 윈도우 마우스 다운시 리스트뷰 셀렉티드 아이템 해제
        /// </summary>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (wasListviewClicked == false)
            {
                VM.gamelogFocusReset();
            }
            else
            {
                wasListviewClicked = false;
            }
        }

        /// <summary>
        /// 윈도우 마우스 다운시 리스트뷰 셀렉티드 아이템 해제
        /// </summary>
        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            wasListviewClicked = false;
        }
        /// <summary>
        /// 윈도우 마우스 다운시 리스트뷰 셀렉티드 아이템 해제
        /// </summary>
        private void ListView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            wasListviewClicked = true;
        }

        /// <summary>
        /// 보드셀 안에 마우스가 들어왔을때 이벤트
        /// </summary>
        private void boardCell_MouseEnter(object sender, MouseEventArgs e)
        {
            int pos = Int32.Parse((string)(((Border)sender).Tag));
            if(VM.gameboardCellMouseEnter(pos))
            {
                this.Cursor = Cursors.Hand;
            }

        }
        /// <summary>
        /// 보드셀 밖으로 마우스가 나갈때 이벤트
        /// </summary>
        private void boardCell_MouseLeave(object sender, MouseEventArgs e)
        {
            int pos = Int32.Parse((string)(((Border)sender).Tag));
            if(VM.gameboardCellMouseLeave(pos))
            {
                this.Cursor = Cursors.Arrow;
            }
        }
        /// <summary>
        ///  보드셀을 좌클릭 할 때 이벤트
        /// </summary>
        private void boardCell_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int pos = Int32.Parse((string)(((Border)sender).Tag));
            if(VM.gameboardCellMouseLeftButtonClick(pos))
            {
                this.Cursor = Cursors.Arrow;
            }
        }


        /// <summary>
        /// 플레이어 위치를 표시하는 보더에 마우스가 들어올 때 이벤트
        /// </summary>
        private void PlayerBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            int pos = Int32.Parse((string)(((Border)sender).Tag));
            if (VM.playerBorderSeletingMouseEnter(pos))
            {
                this.Cursor = Cursors.Hand;
            }
        }
        /// <summary>
        /// 플레이어 위치를 표시하는 보더에 마우스가 나갈 때 이벤트
        /// </summary>
        private void PlayerBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            int pos = Int32.Parse((string)(((Border)sender).Tag));
            if(VM.playerBorderSeletingMouseLeave(pos))
            {
                this.Cursor = Cursors.Arrow;
            }
        }
        /// <summary>
        /// 플레이어 위치를 표시하는 보더에 마우스가 나갈 때 이벤트
        /// </summary>
        private void PlayerBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int pos = Int32.Parse((string)(((Border)sender).Tag));
            if(VM.playerSetInitialPos(pos))
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        /// <summary>
        /// 남은 돌 표시 이미지에 마우스 엔터 이벤트
        /// </summary>
        private void LeftStoneImage_MouseEnter(object sender, MouseEventArgs e)
        {
            Stones curStone = (Stones)(((Image)sender).Tag);
            if(VM.addInitialStone_mouseEnter(curStone))
            {
                this.Cursor = Cursors.Hand;
            }
        }
        /// <summary>
        /// 남은 돌 표시 이미지에 마우스 리브 이벤트
        /// </summary>
        private void LeftStoneImage_MouseLeave(object sender, MouseEventArgs e)
        {
            Stones curStone = (Stones)(((Image)sender).Tag);
            if(VM.addInitialStone_mouseLeave(curStone))
            {
                this.Cursor = Cursors.Arrow;
            }
        }
        /// <summary>
        /// 남은 돌 표시 이미지에 마우스 좌클릭 이벤트 - 해당 돌 추가
        /// </summary>
        private void LeftStoneImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Stones curStone = (Stones)(((Image)sender).Tag);
            VM.addInitialStone(curStone);
        }
        /// <summary>
        /// 남은 돌 표시 이미지에 마우스 우클릭 이벤트 - 해당 돌 제거
        /// </summary>
        private void LeftStoneImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Stones curStone = (Stones)(((Image)sender).Tag);
            VM.removeInitialStone(curStone);
        }

        /// <summary>
        /// 내꺼 소유 돌 표시 이미지에 마우스 엔터 이벤트
        /// </summary>
        private void MyStonesImage_MouseEnter(object sender, MouseEventArgs e)
        {
            object curDataContext = ((Border)sender).DataContext;
            if (curDataContext != null)
            {
                Stones curstone = ((StoneCount)curDataContext).stone;
                if (VM.myStoneMouseEnter(curstone))
                {
                    this.Cursor = Cursors.Hand;
                }
            }
        }
        /// <summary>
        /// 내꺼 소유 돌 표시 이미지에 마우스 리브 이벤트
        /// </summary>
        private void MyStonesImage_MouseLeave(object sender, MouseEventArgs e)
        {
            object curDataContext = ((Border)sender).DataContext;
            if (curDataContext != null)
            {
                Stones curstone = ((StoneCount)curDataContext).stone;
                VM.myStoneMouseLeave(curstone);
            }
            this.Cursor = Cursors.Arrow;
        }
        /// <summary>
        /// 내꺼 소유 돌 표시 이미지에 마우스 좌클릭 이벤트 - (돌 초기화중에는 돌 제거, 게임중에는 해당 돌 선택)
        /// </summary>
        private void MyStonesImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Stones curstone = ((StoneCount)(((Border)sender).DataContext)).stone;
            VM.myStoneMouseLeftButtonClick(curstone);
        }

        
    }

    /// <summary>
    /// 바인딩용 프록시 
    /// </summary>
    public class BindingProxy : Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));
    }

}
