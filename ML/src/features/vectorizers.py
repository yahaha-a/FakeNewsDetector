"""
文本向量化模块，用于将文本转换为特征向量

提供将预处理后的文本转换为机器学习算法可用的数值特征向量的功能。
支持多种向量化方法，包括TF-IDF和Count向量化，可以根据配置调整参数。
"""
from typing import List, Optional, Union, Dict, Any, Callable
import numpy as np
from scipy.sparse import spmatrix

from sklearn.feature_extraction.text import TfidfVectorizer, CountVectorizer
from src.utils.config_loader import CONFIG
from src.utils.logger import logger


class TextVectorizer:
    """文本向量化器
    
    将文本数据转换为机器学习模型可以使用的数值特征向量。
    支持TF-IDF和Count两种向量化方法，并可根据配置调整参数。
    """
    
    def __init__(self, vectorizer_type: str = 'tfidf', stopwords: Optional[List[str]] = None) -> None:
        """
        初始化向量化器
        
        Args:
            vectorizer_type: 向量化器类型，可选 'tfidf' 或 'count'
            stopwords: 停用词列表
            
        Raises:
            ValueError: 不支持的向量化器类型
        """
        self.vectorizer_type = vectorizer_type.lower()
        self.stopwords = stopwords if stopwords else []
        self.vectorizer = None
        
        # 检查向量化器类型是否有效
        if self.vectorizer_type not in ['tfidf', 'count']:
            raise ValueError(f"不支持的向量化器类型: {vectorizer_type}，支持的类型: 'tfidf', 'count'")
        
        # 从配置中加载参数
        if self.vectorizer_type == 'tfidf':
            config = CONFIG['features']['tfidf']
            self.vectorizer = TfidfVectorizer(
                min_df=config.get('min_df', 1),
                max_features=config.get('max_features', None),
                ngram_range=tuple(config.get('ngram_range', [1, 1])),
                stop_words=self.stopwords if config.get('use_stopwords', True) else None,
                norm=config.get('norm', 'l2'),
                use_idf=config.get('use_idf', True)
            )
            logger.info(f"初始化TF-IDF向量化器: min_df={config.get('min_df')}, "
                      f"max_features={config.get('max_features')}, "
                      f"ngram_range={config.get('ngram_range')}, "
                      f"use_stopwords={config.get('use_stopwords', True)}")
        else:  # 'count'
            config = CONFIG['features']['countvec']
            self.vectorizer = CountVectorizer(
                min_df=config.get('min_df', 1),
                max_features=config.get('max_features', None),
                ngram_range=tuple(config.get('ngram_range', [1, 1])),
                stop_words=self.stopwords if config.get('use_stopwords', True) else None,
                binary=config.get('binary', False)
            )
            logger.info(f"初始化Count向量化器: min_df={config.get('min_df')}, "
                      f"max_features={config.get('max_features')}, "
                      f"ngram_range={config.get('ngram_range')}, "
                      f"use_stopwords={config.get('use_stopwords', True)}, "
                      f"binary={config.get('binary', False)}")
    
    def fit_transform(self, texts: List[str], progress_callback: Optional[Callable] = None) -> spmatrix:
        """
        拟合并转换文本数据
        
        对文本数据进行向量化，同时学习词汇表。
        
        Args:
            texts: 待向量化的文本列表
            progress_callback: 进度回调函数
            
        Returns:
            spmatrix: 稀疏特征矩阵
            
        Raises:
            ValueError: 输入文本格式无效或向量化过程出错
        """
        if progress_callback:
            progress_callback(f"{self.vectorizer_type.upper()}向量化")
        
        # 检查输入
        if not texts:
            raise ValueError("输入文本列表为空")
        
        # 过滤无效文本
        valid_texts = [t for t in texts if isinstance(t, str) and t.strip()]
        if len(valid_texts) < len(texts):
            logger.warning(f"过滤了{len(texts) - len(valid_texts)}个无效文本")
        
        if not valid_texts:
            raise ValueError("过滤后没有有效文本可供向量化")
        
        try:
            logger.info(f"使用{self.vectorizer_type}对训练数据进行向量化: {len(valid_texts)}个文档")
            result = self.vectorizer.fit_transform(valid_texts)
            
            # 记录一些统计信息
            vocab_size = len(self.vectorizer.vocabulary_)
            logger.info(f"词汇表大小: {vocab_size}")
            logger.info(f"特征矩阵形状: {result.shape}")
            
            if progress_callback:
                progress_callback(20)
            
            return result
        except Exception as e:
            error_msg = f"{self.vectorizer_type}向量化失败: {str(e)}"
            logger.error(error_msg)
            raise ValueError(error_msg)
    
    def transform(self, texts: List[str], progress_callback: Optional[Callable] = None) -> spmatrix:
        """
        转换文本数据
        
        使用已学习的词汇表对新文本数据进行向量化。
        
        Args:
            texts: 待向量化的文本列表
            progress_callback: 进度回调函数
            
        Returns:
            spmatrix: 稀疏特征矩阵
            
        Raises:
            ValueError: 向量化器未初始化或输入文本格式无效
        """
        if self.vectorizer is None:
            raise ValueError("向量化器未初始化，请先调用fit_transform")
        
        if progress_callback:
            progress_callback(f"{self.vectorizer_type.upper()}向量化")
        
        # 检查输入
        if not texts:
            raise ValueError("输入文本列表为空")
        
        # 过滤无效文本
        valid_texts = [t for t in texts if isinstance(t, str) and t.strip()]
        if len(valid_texts) < len(texts):
            logger.warning(f"过滤了{len(texts) - len(valid_texts)}个无效文本")
        
        if not valid_texts:
            raise ValueError("过滤后没有有效文本可供向量化")
        
        try:
            logger.info(f"使用{self.vectorizer_type}对测试数据进行向量化: {len(valid_texts)}个文档")
            result = self.vectorizer.transform(valid_texts)
            
            # 记录一些统计信息
            logger.info(f"特征矩阵形状: {result.shape}")
            
            if progress_callback:
                progress_callback(20)
            
            return result
        except Exception as e:
            error_msg = f"{self.vectorizer_type}向量化失败: {str(e)}"
            logger.error(error_msg)
            raise ValueError(error_msg)
    
    def get_feature_names(self) -> List[str]:
        """
        获取特征名称（词汇表中的词语）
        
        Returns:
            List[str]: 特征名称列表
            
        Raises:
            ValueError: 向量化器未初始化
        """
        if self.vectorizer is None:
            raise ValueError("向量化器未初始化，请先调用fit_transform")
            
        try:
            # 兼容不同版本的sklearn
            if hasattr(self.vectorizer, 'get_feature_names_out'):
                return self.vectorizer.get_feature_names_out().tolist()
            else:
                return self.vectorizer.get_feature_names()
        except Exception as e:
            logger.error(f"获取特征名称失败: {str(e)}")
            return [] 