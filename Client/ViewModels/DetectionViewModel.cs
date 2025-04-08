using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;

namespace Client.ViewModels;

public partial class DetectionViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _newsContent = string.Empty;

    [ObservableProperty]
    private bool _hasResult = false;

    [ObservableProperty]
    private string _resultText = string.Empty;

    [ObservableProperty]
    private string _resultDescription = string.Empty;

    [ObservableProperty]
    private IBrush _resultColor = Brushes.Gray;
    
    [ObservableProperty]
    private IBrush _resultBackgroundColor = Brushes.Gray;
    
    [ObservableProperty]
    private MaterialIconKind _resultIcon = MaterialIconKind.HelpCircle;

    [ObservableProperty]
    private double _confidence = 0;
    
    [ObservableProperty]
    private string _confidenceDescription = string.Empty;

    [ObservableProperty]
    private IBrush _confidenceColor = Brushes.Gray;

    [ObservableProperty]
    private string _detailedAnalysis = string.Empty;
    
    [ObservableProperty]
    private ObservableCollection<KeyFeature> _keyFeatures = new();

    public bool CanDetect => !string.IsNullOrWhiteSpace(NewsContent);

    [RelayCommand]
    private async Task DetectFakeNews()
    {
        // 模拟检测过程
        IsLoading = true;
        await Task.Delay(1500); // 模拟API调用
        
        // 清空关键特征
        KeyFeatures.Clear();
        
        // 模拟检测结果 (真实应用中应调用ML模块)
        double randomValue = new System.Random().NextDouble();
        Confidence = randomValue * 100;
        
        if (randomValue > 0.7)
        {
            ResultText = "真实新闻";
            ResultDescription = "该新闻很可能是真实的";
            ResultColor = Brushes.Green;
            ResultBackgroundColor = new SolidColorBrush(Color.Parse("#2E8B57"));
            ConfidenceColor = Brushes.Green;
            ResultIcon = MaterialIconKind.CheckCircle;
            ConfidenceDescription = "该内容的真实性评分较高，可信度在专业标准范围内。";
            DetailedAnalysis = "该内容经过分析判定为真实新闻，内容与事实相符，可信度较高。内容中未发现明显的虚假陈述或夸大描述，信息来源可靠，逻辑结构清晰，整体可信度高。";
            
            // 添加关键特征
            KeyFeatures.Add(new KeyFeature { 
                Description = "信息来源可靠", 
                Weight = 0.85, 
                WeightColor = Brushes.Green 
            });
            KeyFeatures.Add(new KeyFeature { 
                Description = "事实陈述准确", 
                Weight = 0.92, 
                WeightColor = Brushes.Green 
            });
            KeyFeatures.Add(new KeyFeature { 
                Description = "内容逻辑一致", 
                Weight = 0.78, 
                WeightColor = Brushes.Green 
            });
        }
        else if (randomValue > 0.4)
        {
            ResultText = "可疑新闻";
            ResultDescription = "该新闻包含一些可疑内容";
            ResultColor = Brushes.Orange;
            ResultBackgroundColor = new SolidColorBrush(Color.Parse("#FF8C00"));
            ConfidenceColor = Brushes.Orange;
            ResultIcon = MaterialIconKind.AlertCircle;
            ConfidenceDescription = "该内容包含一些可能不准确或夸大的信息，建议进一步核实。";
            DetailedAnalysis = "该内容经过分析存在部分可疑信息，某些陈述需要进一步核实。内容中存在一定程度的情感化表达，部分观点缺乏足够的事实支持，建议读者保持警惕并寻求其他信息源进行对比验证。";
            
            // 添加关键特征
            KeyFeatures.Add(new KeyFeature { 
                Description = "部分信息缺乏来源", 
                Weight = 0.65, 
                WeightColor = Brushes.Orange 
            });
            KeyFeatures.Add(new KeyFeature { 
                Description = "存在情感化表达", 
                Weight = 0.58, 
                WeightColor = Brushes.Orange 
            });
            KeyFeatures.Add(new KeyFeature { 
                Description = "数据引用不完整", 
                Weight = 0.45, 
                WeightColor = Brushes.Red 
            });
        }
        else
        {
            ResultText = "虚假新闻";
            ResultDescription = "该新闻很可能是虚假信息";
            ResultColor = Brushes.Red;
            ResultBackgroundColor = new SolidColorBrush(Color.Parse("#B22222"));
            ConfidenceColor = Brushes.Red;
            ResultIcon = MaterialIconKind.CloseCircle;
            ConfidenceDescription = "该内容真实性评分极低，包含众多虚假或误导性信息。";
            DetailedAnalysis = "该内容经过分析判定为虚假新闻，存在多处与事实不符的内容，可信度较低。内容中发现明显的虚构情节、夸大事实或误导性表述，信息来源不明或不可靠，整体可信度低。建议读者不要轻信此类信息，并通过权威渠道获取相关事实。";
            
            // 添加关键特征
            KeyFeatures.Add(new KeyFeature { 
                Description = "信息来源不明", 
                Weight = 0.15, 
                WeightColor = Brushes.Red 
            });
            KeyFeatures.Add(new KeyFeature { 
                Description = "存在明显错误陈述", 
                Weight = 0.08, 
                WeightColor = Brushes.Red 
            });
            KeyFeatures.Add(new KeyFeature { 
                Description = "夸大事实与煽动性表述", 
                Weight = 0.12, 
                WeightColor = Brushes.Red 
            });
        }
        
        HasResult = true;
        IsLoading = false;
    }
    
    [ObservableProperty]
    private bool _isLoading = false;

    [RelayCommand]
    private void ImportFromFile()
    {
        // 实现从文件导入功能
        NewsContent = "这是从文件导入的新闻内容示例...\n\n科学家声称发现新型能源，效率提高1000倍，可在一周内解决全球能源危机。据匿名人士透露，该技术已被多国政府秘密收购，预计将在下个月公开发布。业内专家对此表示怀疑，认为这一消息缺乏可靠来源和科学依据。";
    }

    [RelayCommand]
    private void ClearContent()
    {
        NewsContent = string.Empty;
        HasResult = false;
        KeyFeatures.Clear();
    }
}

public class KeyFeature
{
    public string Description { get; set; } = string.Empty;
    public double Weight { get; set; }
    public IBrush WeightColor { get; set; } = Brushes.Gray;
} 