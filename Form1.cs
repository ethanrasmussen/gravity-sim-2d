namespace gravity_simple;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();

        // enable double buffering:
        SetStyle(ControlStyles.AllPaintingInWmPaint | 
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint, true);
        UpdateStyles();

    }
}
