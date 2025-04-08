"""
模型训练流水线模块，整合数据预处理、特征提取和模型训练

提供端到端的模型训练流程，将预处理过的文本数据通过特征提取后进行模型训练和评估。
支持多种模型类型和特征提取方法，可以使用网格搜索进行超参数优化。
"""
from typing import Dict, Tuple, List, Any, Optional, Union, Callable

import numpy as np
from sklearn.base import BaseEstimator

from src.pipeline.pipeline_builder import create_pipeline, create_grid_search_pipeline
from src.utils.logger import logger
from src.evaluation.metrics import evaluate_model


def train_model_with_pipeline(
    x_train: List[str], 
    y_train: np.ndarray, 
    x_test: List[str], 
    y_test: np.ndarray, 
    model_type: str = 'naive_bayes', 
    vectorizer_type: str = 'tfidf', 
    use_grid_search: bool = False, 
    stopwords: Optional[List[str]] = None, 
    progress_callback: Optional[Callable] = None
) -> Tuple[BaseEstimator, np.ndarray, Dict[str, Any]]:
    """
    使用流水线训练模型
    
    构建特征提取和模型训练的端到端流水线，并对测试集进行预测和评估。
    
    Args:
        x_train: 预处理后的训练文本数据
        y_train: 训练标签
        x_test: 预处理后的测试文本数据
        y_test: 测试标签
        model_type: 模型类型，可选 'naive_bayes', 'random_forest', 'svm', 'logistic'
        vectorizer_type: 向量化器类型，可选 'tfidf', 'count'
        use_grid_search: 是否使用网格搜索进行超参数优化
        stopwords: 停用词列表
        progress_callback: 进度回调函数
        
    Returns:
        Tuple:
            model (BaseEstimator): 训练好的模型
            y_predict (np.ndarray): 预测结果
            results (Dict[str, Any]): 评估结果字典，包含各种性能指标
            
    Raises:
        ValueError: 无效的模型类型或向量化器类型
        RuntimeError: 模型训练或评估失败
    """
    valid_models = ['naive_bayes', 'random_forest', 'svm', 'logistic']
    valid_vectorizers = ['tfidf', 'count']
    
    # 验证输入参数
    if model_type not in valid_models:
        raise ValueError(f"无效的模型类型: {model_type}，有效选项: {valid_models}")
    
    if vectorizer_type not in valid_vectorizers:
        raise ValueError(f"无效的向量化器类型: {vectorizer_type}，有效选项: {valid_vectorizers}")
    
    logger.info(f"使用流水线训练{model_type}模型，向量化方法: {vectorizer_type}")
    
    if progress_callback:
        progress_callback(f"准备{model_type}模型训练")
    
    try:
        # 根据需要创建流水线
        if use_grid_search:
            pipeline = create_grid_search_pipeline(model_type, vectorizer_type, stopwords)
            logger.info("使用网格搜索进行超参数优化")
        else:
            pipeline = create_pipeline(model_type, vectorizer_type, stopwords)
        
        # 训练模型
        if progress_callback:
            progress_callback(f"训练{model_type}模型")
        
        pipeline.fit(x_train, y_train)
        
        if progress_callback:
            progress_callback(20)
        
        # 如果使用网格搜索，则记录最佳参数
        if use_grid_search:
            logger.info(f"最佳参数: {pipeline.best_params_}")
            logger.info(f"最佳交叉验证分数: {pipeline.best_score_:.4f}")
            # 使用最佳模型
            model = pipeline.best_estimator_
        else:
            model = pipeline
        
        # 预测
        if progress_callback:
            progress_callback("预测测试集")
        
        y_predict = model.predict(x_test)
        
        # 记录一些预测统计信息
        unique_values, counts = np.unique(y_predict, return_counts=True)
        logger.info(f"预测结果分布: {dict(zip(unique_values, counts))}")
        
        # 评估模型
        if progress_callback:
            progress_callback("评估模型性能")
        
        results = evaluate_model(y_test, y_predict, progress_callback)
        
        return model, y_predict, results
        
    except Exception as e:
        error_msg = f"模型训练失败: {str(e)}"
        logger.error(error_msg, exc_info=True)
        raise RuntimeError(error_msg) 