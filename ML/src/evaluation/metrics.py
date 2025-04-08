"""
模型评估指标模块

提供用于评估分类模型性能的函数和工具，包括准确率、精确率、召回率、F1分数、ROC曲线和AUC等指标。
支持结果可视化和保存评估结果到文件。
"""
import os
import json
from typing import Dict, List, Tuple, Any, Optional, Union, Callable

import numpy as np
import matplotlib.pyplot as plt
import matplotlib.font_manager as fm
from matplotlib import rcParams
from sklearn.metrics import (
    classification_report, confusion_matrix, roc_curve, auc, 
    accuracy_score, precision_score, recall_score, f1_score
)

from src.utils.config_loader import CONFIG
from src.utils.logger import logger


def set_chinese_font():
    """
    设置matplotlib中文字体支持
    
    尝试多种常见中文字体，解决中文显示为方块的问题
    """
    # 中文字体列表，按优先级排序
    chinese_fonts = ['SimHei', 'Microsoft YaHei', 'SimSun', 'FangSong', 'KaiTi', 
                     'STSong', 'STZhongsong', 'STFangsong', 'STKaiti', 'STHeiti', 
                     'STXihei', 'STLiti', 'STHupo', 'STCaiyun', 'STXinwei']
    font_found = False
    
    for font in chinese_fonts:
        try:
            fm.findfont(font, fallback_to_default=False)
            plt.rcParams['font.sans-serif'] = [font]
            plt.rcParams['axes.unicode_minus'] = False  # 解决负号显示问题
            logger.info(f"使用中文字体: {font}")
            font_found = True
            break
        except:
            continue
    
    if not font_found:
        # 如果找不到中文字体，使用警告提示
        plt.rcParams['font.sans-serif'] = ['DejaVu Sans']
        logger.warning("未找到支持中文的字体，图表中的中文可能显示为方块")


def convert_report_to_dict(report: Dict) -> Dict:
    """
    将分类报告转换为可JSON序列化的字典
    
    sklearn的classification_report返回的字典可能包含numpy数据类型，
    需要转换为原生Python类型以便JSON序列化。
    
    Args:
        report: classification_report生成的字典
        
    Returns:
        Dict: 转换后的纯Python字典
    """
    result = {}
    
    for key, value in report.items():
        if isinstance(value, dict):
            # 递归处理嵌套字典
            result[key] = convert_report_to_dict(value)
        elif isinstance(value, (np.int64, np.int32, np.int16, np.int8)):
            # 将numpy整数转换为Python整数
            result[key] = int(value)
        elif isinstance(value, (np.float64, np.float32, np.float16)):
            # 将numpy浮点数转换为Python浮点数
            result[key] = float(value)
        elif isinstance(value, np.ndarray):
            # 将numpy数组转换为Python列表
            result[key] = value.tolist()
        else:
            # 其他类型保持不变
            result[key] = value
    
    return result


def save_evaluation_results(results: Dict[str, Any]) -> None:
    """
    保存评估结果到JSON文件
    
    将评估结果保存为JSON格式，方便后续分析和报告生成。
    
    Args:
        results: 包含各种评估指标的字典
    """
    # 确保结果目录存在
    os.makedirs('results', exist_ok=True)
    
    # 保存结果到JSON文件
    output_path = 'results/evaluation_results.json'
    with open(output_path, 'w', encoding='utf-8') as f:
        json.dump(results, f, ensure_ascii=False, indent=4)
    
    logger.info(f"评估结果已保存到 {output_path}")


def evaluate_model(
    y_true: np.ndarray, 
    y_pred: np.ndarray, 
    progress_callback: Optional[Callable] = None
) -> Dict[str, Any]:
    """
    评估模型性能
    
    计算各种性能指标，包括准确率、精确率、召回率、F1分数、混淆矩阵和ROC曲线等。
    
    Args:
        y_true: 真实标签
        y_pred: 预测标签
        progress_callback: 进度回调函数
        
    Returns:
        Dict[str, Any]: 包含各种评估指标的字典，包含以下键:
            - accuracy: 准确率
            - precision: 精确率
            - recall: 召回率
            - f1: F1分数
            - classification_report: 分类报告
            - confusion_matrix: 混淆矩阵
            - fpr: ROC曲线的假阳性率
            - tpr: ROC曲线的真阳性率
            - thresholds: ROC曲线的阈值
            - auc: AUC值
    """
    if progress_callback:
        progress_callback("评估模型")
    
    logger.info("开始评估模型")
    
    # 检查标签
    if not isinstance(y_true, np.ndarray):
        y_true = np.array(y_true)
    if not isinstance(y_pred, np.ndarray):
        y_pred = np.array(y_pred)
    
    # 计算基础性能指标
    accuracy = accuracy_score(y_true, y_pred)
    precision = precision_score(y_true, y_pred, average='weighted')
    recall = recall_score(y_true, y_pred, average='weighted')
    f1 = f1_score(y_true, y_pred, average='weighted')
    
    logger.info(f"准确率: {accuracy:.4f}")
    logger.info(f"精确率: {precision:.4f}")
    logger.info(f"召回率: {recall:.4f}")
    logger.info(f"F1分数: {f1:.4f}")
    
    # 生成分类报告
    report = classification_report(y_true, y_pred, output_dict=True)
    logger.info(f"分类报告:\n{classification_report(y_true, y_pred)}")
    
    # 计算混淆矩阵
    conf_matrix = confusion_matrix(y_true, y_pred)
    logger.info(f"混淆矩阵:\n{conf_matrix}")
    
    # 计算ROC曲线和AUC值
    try:
        fpr, tpr, thresholds = roc_curve(y_true, y_pred, pos_label=1)
        auc_value = auc(fpr, tpr)
        logger.info(f"AUC: {auc_value:.4f}")
    except Exception as e:
        logger.warning(f"计算ROC曲线失败: {str(e)}，使用默认值")
        fpr, tpr, thresholds = np.array([0, 1]), np.array([0, 1]), np.array([1, 0])
        auc_value = 0.5
    
    # 绘制ROC曲线
    if CONFIG['evaluation'].get('plot_roc', True):
        plot_roc_curve(fpr, tpr, auc_value)
    
    # 绘制混淆矩阵
    if CONFIG['evaluation'].get('plot_confusion_matrix', True):
        plot_confusion_matrix(conf_matrix)
    
    if progress_callback:
        progress_callback(20)
    
    # 保存评估结果到文件
    if CONFIG['evaluation'].get('save_results', True):
        save_evaluation_results({
            'accuracy': float(accuracy),
            'precision': float(precision),
            'recall': float(recall),
            'f1': float(f1),
            'auc': float(auc_value),
            'classification_report': convert_report_to_dict(report)
        })
    
    # 返回评估结果
    results = {
        'accuracy': accuracy,
        'precision': precision,
        'recall': recall,
        'f1': f1,
        'classification_report': report,
        'confusion_matrix': conf_matrix,
        'fpr': fpr,
        'tpr': tpr,
        'thresholds': thresholds,
        'auc': auc_value
    }
    
    logger.info("模型评估完成")
    return results


def plot_roc_curve(fpr: np.ndarray, tpr: np.ndarray, auc_value: float) -> None:
    """
    绘制ROC曲线
    
    绘制ROC曲线并保存为图像文件。
    
    Args:
        fpr: 假阳性率
        tpr: 真阳性率
        auc_value: AUC值
    """
    # 设置中文字体
    set_chinese_font()
    
    plt.figure(figsize=(10, 8))
    plt.title('ROC曲线 (AUC = {:.4f})'.format(auc_value))
    plt.xlabel('假阳性率')
    plt.ylabel('真阳性率')
    plt.plot(fpr, tpr, color='b', lw=2, label='ROC曲线')
    plt.plot([0, 1], [0, 1], color='r', linestyle='--', label='随机猜测')
    plt.xlim([0.0, 1.0])
    plt.ylim([0.0, 1.05])
    plt.legend(loc='lower right')
    plt.grid(True)
    
    # 创建结果目录
    os.makedirs('results', exist_ok=True)
    
    # 保存图像
    plt.savefig('results/roc_curve.png', dpi=300, bbox_inches='tight')
    logger.info("ROC曲线已保存到 results/roc_curve.png")
    
    # 关闭图像
    plt.close()
    

def plot_confusion_matrix(conf_matrix: np.ndarray) -> None:
    """
    绘制混淆矩阵
    
    将混淆矩阵可视化并保存为图像文件。
    
    Args:
        conf_matrix: 混淆矩阵数组
    """
    # 设置中文字体
    set_chinese_font()
    
    plt.figure(figsize=(8, 6))
    plt.imshow(conf_matrix, interpolation='nearest', cmap=plt.cm.Blues)
    plt.title('混淆矩阵')
    plt.colorbar()
    
    classes = ['真实', '虚假']
    tick_marks = np.arange(len(classes))
    plt.xticks(tick_marks, classes, rotation=45)
    plt.yticks(tick_marks, classes)
    
    # 在每个单元格中显示数值
    thresh = conf_matrix.max() / 2.
    for i in range(conf_matrix.shape[0]):
        for j in range(conf_matrix.shape[1]):
            plt.text(j, i, format(conf_matrix[i, j], 'd'),
                    horizontalalignment="center",
                    color="white" if conf_matrix[i, j] > thresh else "black")
    
    plt.tight_layout()
    plt.ylabel('真实标签')
    plt.xlabel('预测标签')
    
    # 创建结果目录
    os.makedirs('results', exist_ok=True)
    
    # 保存图像
    plt.savefig('results/confusion_matrix.png', dpi=300, bbox_inches='tight')
    logger.info("混淆矩阵已保存到 results/confusion_matrix.png")
    
    # 关闭图像
    plt.close() 