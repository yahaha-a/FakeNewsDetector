"""
FakeNewsDetector - 中文虚假新闻检测系统主程序
"""
import os
import argparse
from typing import Dict, Tuple, List, Any, Optional, Union
from tqdm import tqdm

from src.utils.config_loader import CONFIG, load_config
from src.utils.logger import logger
from src.data.data_loader import load_data
from src.data.preprocessor import TextPreprocessor
from src.pipeline.model_trainer import train_model_with_pipeline
from src.evaluation.metrics import plot_roc_curve


def main() -> None:
    """
    FakeNewsDetector主程序入口函数，处理命令行参数，加载数据，预处理，训练模型并评估结果。
    
    命令行参数:
        --model: 选择使用的模型类型 (naive_bayes, random_forest, svm, logistic)
        --vectorizer: 选择使用的特征向量化方法 (tfidf, count)
        --grid-search: 是否使用网格搜索进行超参数优化
        --config: 配置文件路径
    """
    # 解析命令行参数
    parser = argparse.ArgumentParser(description="FakeNewsDetector - 中文虚假新闻检测系统")
    parser.add_argument('--model', type=str, default='naive_bayes',
                        choices=['naive_bayes', 'random_forest', 'svm', 'logistic'],
                        help="选择要使用的模型")
    parser.add_argument('--vectorizer', type=str, default='tfidf',
                        choices=['tfidf', 'count'],
                        help="选择要使用的向量化方法")
    parser.add_argument('--grid-search', action='store_true',
                        help="是否使用网格搜索进行超参数优化")
    parser.add_argument('--config', type=str, default='config/config.yaml',
                        help="配置文件路径")
    
    args = parser.parse_args()
    
    # 如果指定了配置文件，重新加载
    if args.config != 'config/config.yaml':
        global CONFIG
        CONFIG = load_config(args.config)
    
    # 创建结果目录
    os.makedirs('results', exist_ok=True)
    
    # 设置进度条
    pbar = tqdm(total=100, desc="初始化", ncols=100)
    
    def update_progress(value_or_desc: Union[int, str]) -> None:
        """更新进度条
        
        Args:
            value_or_desc: 进度增量或描述文本
        """
        if isinstance(value_or_desc, str):
            pbar.set_description(value_or_desc)
        else:
            pbar.update(value_or_desc)
    
    logger.info(f"启动FakeNewsDetector，使用模型: {args.model}，向量化方法: {args.vectorizer}")
    
    try:
        # 加载数据
        update_progress("加载数据")
        x_train, y_train, x_test, y_test, stopwords = load_data()
        
        # 预处理数据
        update_progress("预处理数据")
        preprocessor = TextPreprocessor(stopwords)
        x_train_processed, x_test_processed = preprocessor.preprocess_data(
            x_train, x_test, update_progress)
        
        # 使用流水线训练模型
        update_progress(f"训练{args.model}模型")
        model, y_predict, results = train_model_with_pipeline(
            x_train_processed, y_train, x_test_processed, y_test,
            model_type=args.model,
            vectorizer_type=args.vectorizer,
            use_grid_search=args.grid_search,
            stopwords=stopwords,
            progress_callback=update_progress
        )
        
        # 显示结果
        logger.info(f"模型评估结果: 准确率={results['accuracy']:.4f}, AUC={results['auc']:.4f}")
        print("\n模型评估结果:")
        print(f"准确率: {results['accuracy']:.4f}")
        print(f"AUC: {results['auc']:.4f}")
        
        # 关闭进度条
        pbar.close()
        logger.info("FakeNewsDetector执行完成")
        
    except FileNotFoundError as e:
        logger.error(f"文件未找到: {str(e)}", exc_info=True)
        pbar.close()
        print(f"错误: 无法找到所需文件 - {str(e)}")
        
    except ValueError as e:
        logger.error(f"数据处理错误: {str(e)}", exc_info=True)
        pbar.close()
        print(f"错误: 数据处理异常 - {str(e)}")
        
    except Exception as e:
        logger.error(f"发生未知错误: {str(e)}", exc_info=True)
        pbar.close()
        print(f"错误: 系统执行异常 - {str(e)}")
        raise


if __name__ == "__main__":
    main()
