"""
FakeNewsDetector - 中文虚假新闻检测系统主程序
"""
import os
import sys
import argparse
import traceback
from typing import Dict, Tuple, List, Any, Optional, Union
from tqdm import tqdm

from src.utils.config_loader import CONFIG, load_config
from src.utils.logger import logger
from src.data.data_loader import load_data
from src.data.preprocessor import TextPreprocessor
from src.features.vectorizers import TextVectorizer
from src.models import train_naive_bayes, train_random_forest, train_svm, train_logistic_regression
from src.evaluation.metrics import evaluate_model, plot_roc_curve


def main() -> None:
    """
    FakeNewsDetector主程序入口函数，处理命令行参数，加载数据，预处理，训练模型并评估结果。
    
    命令行参数:
        --model: 选择使用的模型类型 (naive_bayes, random_forest, svm, logistic)
        --vectorizer: 选择使用的特征向量化方法 (tfidf, count)
        --config: 配置文件路径
    """
    # 解析命令行参数
    parser = argparse.ArgumentParser(description="FakeNewsDetector - 中文虚假新闻检测系统")
    parser.add_argument('--model', type=str, default=None,
                        choices=['naive_bayes', 'random_forest', 'svm', 'logistic'],
                        help="选择要使用的模型")
    parser.add_argument('--vectorizer', type=str, default=None,
                        choices=['tfidf', 'count'],
                        help="选择要使用的向量化方法")
    parser.add_argument('--config', type=str, default='config/config.yaml',
                        help="配置文件路径")
    
    args = parser.parse_args()
    
    # 交互式选择模型（如果未通过命令行指定）
    if args.model is None:
        print("\n请选择要使用的模型:")
        print("1. 朴素贝叶斯 (naive_bayes)")
        print("2. 随机森林 (random_forest)")
        print("3. 支持向量机 (svm)")
        print("4. 逻辑回归 (logistic)")
        
        choice = input("\n请输入选项编号 [1-4]，默认为1: ").strip()
        
        model_map = {
            "": "naive_bayes",  # 默认选项
            "1": "naive_bayes",
            "2": "random_forest",
            "3": "svm",
            "4": "logistic"
        }
        
        if choice not in model_map:
            print(f"无效的选择: {choice}，将使用默认模型 (朴素贝叶斯)")
            args.model = "naive_bayes"
        else:
            args.model = model_map[choice]
            
        print(f"已选择模型: {args.model}")
    
    # 交互式选择向量化方法（如果未通过命令行指定）
    if args.vectorizer is None:
        print("\n请选择要使用的向量化方法:")
        print("1. TF-IDF向量化 (tfidf)")
        print("2. Count向量化 (count)")
        
        choice = input("\n请输入选项编号 [1-2]，默认为1: ").strip()
        
        vectorizer_map = {
            "": "tfidf",  # 默认选项
            "1": "tfidf",
            "2": "count"
        }
        
        if choice not in vectorizer_map:
            print(f"无效的选择: {choice}，将使用默认向量化方法 (tfidf)")
            args.vectorizer = "tfidf"
        else:
            args.vectorizer = vectorizer_map[choice]
            
        print(f"已选择向量化方法: {args.vectorizer}")
    
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
            logger.debug(f"进度更新: {value_or_desc}")
        else:
            pbar.update(value_or_desc)
    
    logger.info(f"启动FakeNewsDetector，使用模型: {args.model}，向量化方法: {args.vectorizer}")
    
    try:
        # 加载数据
        update_progress("加载数据")
        x_train, y_train, x_test, y_test, stopwords = load_data()
        logger.info(f"数据加载完成，训练集大小: {len(x_train)}，测试集大小: {len(x_test)}")
        update_progress(10)  # 为数据加载分配10%进度
        
        # 预处理数据
        update_progress("预处理数据")
        preprocessor = TextPreprocessor(stopwords)
        x_train_processed, x_test_processed = preprocessor.preprocess_data(
            x_train, x_test, update_progress)
        logger.info(f"数据预处理完成，处理后训练集大小: {len(x_train_processed)}，测试集大小: {len(x_test_processed)}")
        update_progress(10)  # 为数据预处理分配10%进度
        
        # 使用向量化器
        update_progress(f"特征提取: {args.vectorizer}")
        vectorizer = TextVectorizer(args.vectorizer, stopwords)
        x_train_vec = vectorizer.fit_transform(x_train_processed, update_progress)
        x_test_vec = vectorizer.transform(x_test_processed, update_progress)
        logger.info(f"特征提取完成，特征矩阵形状: {x_train_vec.shape}, {x_test_vec.shape}")
        update_progress(10)  # 为特征提取分配10%进度
        
        # 训练模型
        update_progress(f"训练{args.model}模型")
        logger.info(f"开始训练{args.model}模型...")
        
        # 各个模型已经在各自的函数中有进度更新，此处只分配模型初始化的部分进度
        update_progress(5)  # 为模型初始化分配5%进度
        
        if args.model == 'naive_bayes':
            model = train_naive_bayes(x_train_vec, y_train, update_progress)
            # 朴素贝叶斯在函数内部会更新5%进度
        elif args.model == 'random_forest':
            model = train_random_forest(x_train_vec, y_train, update_progress)
            # 随机森林在函数内部会更新总共20%进度(5%+15%)
        elif args.model == 'svm':
            model = train_svm(x_train_vec, y_train, update_progress)
            # SVM在函数内部会更新10%进度
        elif args.model == 'logistic':
            model = train_logistic_regression(x_train_vec, y_train, update_progress)
            # 逻辑回归在函数内部会更新8%进度
        
        logger.info("模型训练完成")
        
        # 根据模型类型补充剩余进度，确保总进度达到30%
        remaining_progress = {
            'naive_bayes': 20,    # 已经更新了5% + 5% = 10%，还需要20%
            'random_forest': 5,   # 已经更新了5% + 20% = 25%，还需要5%
            'svm': 15,            # 已经更新了5% + 10% = 15%，还需要15%
            'logistic': 17        # 已经更新了5% + 8% = 13%，还需要17%
        }
        update_progress(remaining_progress[args.model])  # 确保所有模型总进度为30%
        
        # 预测
        update_progress("预测测试集")
        y_predict = model.predict(x_test_vec)
        logger.info(f"预测完成，预测结果大小: {len(y_predict)}")
        update_progress(10)  # 为预测分配10%进度
        
        # 评估模型
        update_progress("评估模型性能")
        results = evaluate_model(y_test, y_predict, update_progress)
        logger.info(f"模型评估完成")
        update_progress(10)  # 为评估分配10%进度
        
        # 绘制ROC曲线（不需要这里调用，因为evaluate_model内部已经调用了）
        update_progress("完成")
        update_progress(10)  # 最后10%进度完成
        
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
        print(traceback.format_exc())
        
    except ValueError as e:
        logger.error(f"数据处理错误: {str(e)}", exc_info=True)
        pbar.close()
        print(f"错误: 数据处理异常 - {str(e)}")
        print(traceback.format_exc())
        
    except Exception as e:
        logger.error(f"发生未知错误: {str(e)}", exc_info=True)
        pbar.close()
        print(f"错误: 系统执行异常 - {str(e)}")
        print(traceback.format_exc())
        raise


if __name__ == "__main__":
    try:
        logger.debug("程序开始执行...")
        main()
    except Exception as e:
        print(f"程序执行出错: {str(e)}")
        traceback.print_exc()
        sys.exit(1)
