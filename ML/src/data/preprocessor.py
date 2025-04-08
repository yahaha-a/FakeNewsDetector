"""
数据预处理模块，用于文本处理和特征提取

提供用于处理原始文本数据的类和函数，包括分词、停用词过滤和文本特征提取等操作。
此模块是模型训练的关键预处理步骤，可以显著影响模型性能。
"""
import pandas as pd
import jieba
from typing import List, Tuple, Callable, Optional, Union, Dict, Any
from tqdm import tqdm
from src.utils.config_loader import CONFIG
from src.utils.logger import logger


class TextPreprocessor:
    """文本预处理器

    用于对文本数据进行预处理，包括分词、停用词过滤和特征提取。
    支持使用jieba和paddle两种分词方式。
    """
    
    def __init__(self, stopwords: Optional[List[str]] = None) -> None:
        """
        初始化预处理器
        
        Args:
            stopwords: 停用词列表，如果为None则不使用停用词
        """
        self.stopwords = stopwords if stopwords else []
        self.use_stopwords = CONFIG['preprocessing']['use_stopwords']
        self.content_separator = CONFIG['preprocessing']['content_separator']
        self.tokenizer = CONFIG['preprocessing']['tokenizer']
        
        # 初始化分词器
        if self.tokenizer == 'paddle':
            try:
                # 尝试加载paddle模式
                jieba.enable_paddle()
                logger.info("成功启用paddle模式分词")
            except Exception as e:
                logger.warning(f"无法启用paddle模式分词，将使用默认模式: {str(e)}")
                self.tokenizer = 'jieba'
        
        logger.info(f"初始化文本预处理器: 使用停用词={self.use_stopwords}, 分词器={self.tokenizer}")
    
    def tokenize_text(self, text: str) -> str:
        """
        对文本进行分词
        
        使用指定的分词器对文本进行分词，并可选地过滤停用词。
        
        Args:
            text: 待分词文本
            
        Returns:
            str: 分词后的文本，以空格分隔
            
        Raises:
            ValueError: 文本格式无效或分词过程出错
        """
        if not text or not isinstance(text, str):
            return ""
            
        try:
            if self.tokenizer == 'paddle':
                words = list(jieba.cut(text, use_paddle=True))
            else:
                words = list(jieba.cut(text))
            
            if self.use_stopwords and self.stopwords:
                # 过滤停用词
                words = [word for word in words if word not in self.stopwords and word.strip()]
            
            return " ".join(words)
        except Exception as e:
            logger.error(f"分词失败: {str(e)}, 文本: {text[:100]}...")
            return ""
    
    def preprocess_data(self, x_train: pd.DataFrame, x_test: pd.DataFrame, 
                        progress_callback: Optional[Callable] = None) -> Tuple[List[str], List[str]]:
        """
        预处理训练和测试数据
        
        对训练集和测试集的文本进行预处理，包括分词、整合特征等。
        
        Args:
            x_train: 训练特征
            x_test: 测试特征
            progress_callback: 进度回调函数，用于更新UI进度
            
        Returns:
            Tuple[List[str], List[str]]: 处理后的训练数据和测试数据
            
        Raises:
            ValueError: 数据格式无效或预处理过程出错
        """
        logger.info("开始数据预处理")
        
        # 创建数据副本以避免警告
        x_train = x_train.copy()
        x_test = x_test.copy()
        
        if progress_callback:
            progress_callback("预处理数据")
        
        try:
            # 评论分条处理
            logger.info("处理报告内容分隔符")
            self._process_report_content(x_train, "Report Content")
            self._process_report_content(x_test, "Report Content")
            
            if progress_callback:
                progress_callback(20)
            
            # 整合特征
            if progress_callback:
                progress_callback("中文分词")
            
            # 处理训练数据
            logger.info("对训练数据进行特征整合和分词")
            x_train_processed = self._integrate_and_tokenize_features(x_train)
            
            if progress_callback:
                progress_callback(20)
            
            # 处理测试数据
            logger.info("对测试数据进行特征整合和分词")
            x_test_processed = self._integrate_and_tokenize_features(x_test)
            
            if progress_callback:
                progress_callback(20)
            
            # 输出一些数据样例以便调试
            if logger.level <= 10:  # DEBUG级别
                logger.debug(f"预处理后的训练数据样例: {x_train_processed[0][:100]}...")
                logger.debug(f"预处理后的测试数据样例: {x_test_processed[0][:100]}...")
            
            logger.info("数据预处理完成")
            return x_train_processed, x_test_processed
            
        except Exception as e:
            logger.error(f"数据预处理失败: {str(e)}")
            raise ValueError(f"数据预处理错误: {str(e)}")
    
    def _process_report_content(self, data: pd.DataFrame, column: str) -> None:
        """
        处理报告内容的分隔符
        
        将报告内容按分隔符分割后重新连接为单个文本。
        
        Args:
            data: 数据集
            column: 报告内容列名
        """
        if column not in data.columns:
            logger.warning(f"列 {column} 不存在于数据集中")
            return
            
        data.loc[:, column] = data[column].apply(
            lambda x: x.split(self.content_separator) if isinstance(x, str) else [])
        data.loc[:, column] = data[column].apply(
            lambda x: " ".join(x) if isinstance(x, list) else "")
    
    def _integrate_and_tokenize_features(self, data: pd.DataFrame) -> List[str]:
        """
        整合并分词特征
        
        将标题、官方账号名和报告内容合并为一个特征，并进行分词。
        
        Args:
            data: 数据集
            
        Returns:
            List[str]: 分词后的特征列表
        """
        # 准备好整合后的特征列
        data.loc[:, "integrated_features"] = ""
        
        # 将所有字段转为字符串类型
        t = pd.DataFrame(data.astype(str))
        
        # 整合特征
        if "Title" in t.columns and "Ofiicial Account Name" in t.columns and "Report Content" in t.columns:
            data.loc[:, "integrated_features"] = (
                t["Title"] + ' ' + t["Ofiicial Account Name"] + ' ' + t["Report Content"]
            )
        else:
            available_columns = [col for col in ["Title", "Ofiicial Account Name", "Report Content"] if col in t.columns]
            logger.warning(f"部分特征列不存在，仅使用: {available_columns}")
            if available_columns:
                data.loc[:, "integrated_features"] = t[available_columns].apply(
                    lambda row: ' '.join(row), axis=1
                )
            else:
                raise ValueError("无可用特征列")
        
        # 分词
        processed_data = []
        total = len(data)
        
        for i, text in enumerate(data["integrated_features"]):
            processed_text = self.tokenize_text(text)
            processed_data.append(processed_text)
            
            # 每处理10%的数据记录一次日志
            if i % max(1, total // 10) == 0:
                logger.info(f"分词进度: {i}/{total}")
        
        return processed_data 