"""
数据加载模块，用于加载训练和测试数据集及停用词

提供用于加载虚假新闻数据集和中文停用词的函数。
支持从配置中指定的文件路径读取数据，并处理基本的文件格式错误。
"""
import os
from typing import Tuple, List, Any, Optional

import pandas as pd
import numpy as np
from src.utils.config_loader import CONFIG
from src.utils.logger import logger


def load_data() -> Tuple[pd.DataFrame, np.ndarray, pd.DataFrame, np.ndarray, List[str]]:
    """
    加载数据和停用词
    
    从配置文件指定的路径加载训练集、测试集和停用词。
    训练集和测试集应为CSV格式，停用词为文本文件，每行一个词。
    
    Returns:
        x_train (pd.DataFrame): 训练特征
        y_train (np.ndarray): 训练标签
        x_test (pd.DataFrame): 测试特征
        y_test (np.ndarray): 测试标签
        stopwords (List[str]): 停用词列表
        
    Raises:
        FileNotFoundError: 文件路径不存在
        pd.errors.EmptyDataError: CSV文件为空
        pd.errors.ParserError: CSV文件格式错误
    """
    logger.info("开始加载数据")
    
    # 获取数据路径
    train_data_path = CONFIG['data']['train_path']
    test_data_path = CONFIG['data']['test_path']
    stopwords_path = CONFIG['data']['stopwords_path']
    
    # 检查文件是否存在
    for file_path in [train_data_path, test_data_path, stopwords_path]:
        if not os.path.exists(file_path):
            error_msg = f"文件不存在: {file_path}"
            logger.error(error_msg)
            raise FileNotFoundError(error_msg)
    
    # 加载训练和测试数据
    try:
        train_data = pd.read_csv(train_data_path)
        test_data = pd.read_csv(test_data_path)
        
        # 验证数据集不为空
        if train_data.empty or test_data.empty:
            error_msg = "训练集或测试集为空"
            logger.error(error_msg)
            raise ValueError(error_msg)
            
        # 检查数据集格式是否正确
        required_columns = ['Title', 'Ofiicial Account Name', 'Report Content']
        for dataset, name in [(train_data, "训练集"), (test_data, "测试集")]:
            missing_cols = [col for col in required_columns if col not in dataset.columns]
            if missing_cols:
                error_msg = f"{name}缺少必要列: {', '.join(missing_cols)}"
                logger.error(error_msg)
                raise ValueError(error_msg)
        
        logger.info(f"成功加载训练数据: {train_data.shape[0]}行, {train_data.shape[1]}列")
        logger.info(f"成功加载测试数据: {test_data.shape[0]}行, {test_data.shape[1]}列")
    except pd.errors.EmptyDataError:
        error_msg = "CSV文件为空"
        logger.error(error_msg)
        raise
    except pd.errors.ParserError:
        error_msg = "CSV文件格式错误"
        logger.error(error_msg)
        raise
    except Exception as e:
        logger.error(f"加载数据失败: {str(e)}")
        raise
    
    # 加载停用词
    try:
        with open(stopwords_path, 'r', encoding='utf-8') as f:
            stopwords = [line.strip() for line in f if line.strip()]
        
        logger.info(f"成功加载停用词: {len(stopwords)}个")
    except Exception as e:
        logger.error(f"加载停用词失败: {str(e)}")
        raise
    
    # 提取特征和标签
    x_train = extract_features(train_data)
    y_train = extract_labels(train_data)
    x_test = extract_features(test_data)
    y_test = extract_labels(test_data)
    
    logger.info("数据加载完成")
    return x_train, y_train, x_test, y_test, stopwords


def extract_features(data: pd.DataFrame) -> pd.DataFrame:
    """
    从数据集中提取特征
    
    从原始数据框中提取用于模型训练的特征列
    
    Args:
        data: 原始数据集
    
    Returns:
        pd.DataFrame: 包含特征列的数据框（标题、官方账号名和报告内容）
    """
    # 选择标题、官方账号名和报告内容作为特征
    feature_columns = ['Title', 'Ofiicial Account Name', 'Report Content']
    return data[feature_columns].copy()


def extract_labels(data: pd.DataFrame) -> np.ndarray:
    """
    从数据集中提取标签
    
    从原始数据框中提取标签列（第6列，索引为5）
    
    Args:
        data: 原始数据集
    
    Returns:
        np.ndarray: 标签数组
    """
    # 标签在第6列（索引为5）
    label_column = data.columns[5]
    logger.debug(f"提取标签列: {label_column}")
    return np.array(data[label_column])