"""
配置加载模块

提供加载和管理系统配置的功能，支持从YAML文件加载配置，
并提供默认配置和配置验证。
"""
import os
import sys
import yaml
from typing import Dict, Any, Optional, List


class ConfigLoader:
    """配置加载器类
    
    负责从文件加载配置并管理配置内容。
    支持配置验证和默认配置设置。
    """
    
    def __init__(self, config_path: str = "config/config.yaml") -> None:
        """
        初始化配置加载器
        
        Args:
            config_path: 配置文件路径
            
        Raises:
            FileNotFoundError: 配置文件不存在
            yaml.YAMLError: 配置文件格式错误
        """
        self.config_path = config_path
        self.config = self._load_config()
        
        # 基本配置验证
        self._validate_config()
    
    def _load_config(self) -> Dict[str, Any]:
        """
        从文件加载YAML配置
        
        Returns:
            Dict[str, Any]: 加载的配置字典
            
        Raises:
            FileNotFoundError: 配置文件不存在
            yaml.YAMLError: 配置文件格式错误
        """
        # 检查配置文件是否存在
        if not os.path.exists(self.config_path):
            error_msg = f"配置文件不存在: {self.config_path}"
            print(f"错误: {error_msg}")
            raise FileNotFoundError(error_msg)
        
        try:
            with open(self.config_path, 'r', encoding='utf-8') as f:
                config = yaml.safe_load(f)
                
                # 打印配置加载信息
                print(f"已成功加载配置文件: {self.config_path}")
                
                return config
        except yaml.YAMLError as e:
            error_msg = f"YAML配置解析错误: {str(e)}"
            print(f"错误: {error_msg}")
            raise
    
    def _validate_config(self) -> None:
        """
        验证配置的完整性和正确性
        
        验证配置中是否包含必要的部分，并检查值的合法性。
        如果缺少必要的配置，则使用默认值填充。
        
        Raises:
            ValueError: 配置验证失败且无法使用默认值
        """
        # 必要的配置部分
        required_sections = ['data', 'preprocessing', 'features', 'model', 'evaluation', 'logging']
        
        for section in required_sections:
            if section not in self.config:
                print(f"警告: 配置中缺少 '{section}' 部分，将使用默认值")
                self.config[section] = {}
        
        # 设置默认值
        self._set_defaults()
    
    def _set_defaults(self) -> None:
        """设置各部分的默认配置值"""
        # 数据部分默认值
        if 'data' in self.config:
            data_defaults = {
                'train_path': 'data/train.news.csv',
                'test_path': 'data/test.news.csv',
                'stopwords_path': 'data/stop_words.txt'
            }
            for key, value in data_defaults.items():
                if key not in self.config['data']:
                    self.config['data'][key] = value
        
        # 预处理部分默认值
        if 'preprocessing' in self.config:
            preprocessing_defaults = {
                'use_stopwords': True,
                'content_separator': '\n',
                'tokenizer': 'jieba'
            }
            for key, value in preprocessing_defaults.items():
                if key not in self.config['preprocessing']:
                    self.config['preprocessing'][key] = value
        
        # 特征部分默认值
        if 'features' in self.config:
            if 'tfidf' not in self.config['features']:
                self.config['features']['tfidf'] = {
                    'min_df': 5,
                    'max_features': 10000,
                    'ngram_range': [1, 2],
                    'use_stopwords': True,
                    'use_idf': True,
                    'norm': 'l2'
                }
            
            if 'countvec' not in self.config['features']:
                self.config['features']['countvec'] = {
                    'min_df': 5,
                    'max_features': 10000,
                    'ngram_range': [1, 2],
                    'use_stopwords': True,
                    'binary': False
                }
        
        # 模型部分默认值
        if 'model' in self.config:
            model_defaults = {
                'random_state': 42,
                'test_size': 0.2
            }
            for key, value in model_defaults.items():
                if key not in self.config['model']:
                    self.config['model'][key] = value
        
        # 日志部分默认值
        if 'logging' in self.config:
            logging_defaults = {
                'log_level': 'INFO',
                'log_file': 'logs/application.log',
                'log_to_console': True
            }
            for key, value in logging_defaults.items():
                if key not in self.config['logging']:
                    self.config['logging'][key] = value
    
    def get_config(self) -> Dict[str, Any]:
        """
        获取完整配置
        
        Returns:
            Dict[str, Any]: 配置字典
        """
        return self.config
    
    def get_section(self, section: str) -> Dict[str, Any]:
        """
        获取指定配置部分
        
        Args:
            section: 配置部分名称
            
        Returns:
            Dict[str, Any]: 指定部分的配置字典
            
        Raises:
            KeyError: 指定的配置部分不存在
        """
        if section not in self.config:
            raise KeyError(f"配置中不存在部分: {section}")
        return self.config[section]
    
    def save_config(self, config_path: Optional[str] = None) -> None:
        """
        保存当前配置到文件
        
        Args:
            config_path: 保存的配置文件路径，默认为加载时的路径
            
        Raises:
            OSError: 保存配置文件失败
        """
        path = config_path or self.config_path
        
        # 确保目录存在
        os.makedirs(os.path.dirname(path), exist_ok=True)
        
        try:
            with open(path, 'w', encoding='utf-8') as f:
                yaml.dump(self.config, f, default_flow_style=False, allow_unicode=True)
            print(f"配置已保存到: {path}")
        except OSError as e:
            print(f"保存配置失败: {str(e)}")
            raise


def load_config(config_path: str) -> Dict[str, Any]:
    """
    从指定路径加载配置
    
    Args:
        config_path: 配置文件路径
    
    Returns:
        Dict[str, Any]: 加载的配置字典
    
    Raises:
        FileNotFoundError: 配置文件不存在
        yaml.YAMLError: 配置文件格式错误
    """
    loader = ConfigLoader(config_path)
    return loader.get_config()


# 全局配置实例
try:
    config_loader = ConfigLoader()
    CONFIG = config_loader.get_config()
except Exception as e:
    print(f"加载配置失败: {str(e)}")
    sys.exit(1) 