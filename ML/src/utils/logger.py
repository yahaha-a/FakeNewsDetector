"""
日志记录模块

提供统一的日志记录功能，支持控制台和文件输出，支持不同的日志级别。
可通过配置文件调整日志级别和输出方式。
"""
import os
import sys
import logging
from typing import Optional, Dict, Any, Union
from logging.handlers import RotatingFileHandler

from src.utils.config_loader import CONFIG


class LoggerConfig:
    """日志记录器配置类
    
    用于管理和配置日志系统，支持控制台和文件输出，
    可设置不同的日志级别和格式。
    """
    
    LOG_LEVELS = {
        'debug': logging.DEBUG,
        'info': logging.INFO,
        'warning': logging.WARNING,
        'error': logging.ERROR,
        'critical': logging.CRITICAL
    }
    
    def __init__(self, config: Optional[Dict[str, Any]] = None) -> None:
        """
        初始化日志配置
        
        Args:
            config: 日志配置字典，默认使用全局配置中的日志设置
        """
        self.config = config or CONFIG.get('logging', {})
        
        # 默认配置
        self.default_config = {
            'log_level': 'info',
            'log_file': 'logs/application.log',
            'log_to_console': True,
            'log_format': '%(asctime)s - %(name)s - %(levelname)s - %(message)s',
            'max_log_size': 10 * 1024 * 1024,  # 10MB
            'backup_count': 5,
            'encoding': 'utf-8'
        }
        
        # 使用配置覆盖默认值
        for key, value in self.default_config.items():
            if key not in self.config:
                self.config[key] = value
        
        # 设置日志级别
        self.log_level = self.LOG_LEVELS.get(
            self.config['log_level'].lower(), 
            logging.INFO
        )
        
        # 日志文件路径
        self.log_file = self.config['log_file']
        
        # 确保日志目录存在
        log_dir = os.path.dirname(self.log_file)
        if log_dir and not os.path.exists(log_dir):
            os.makedirs(log_dir)
    
    def get_logger(self, name: str) -> logging.Logger:
        """
        获取配置好的日志记录器
        
        Args:
            name: 日志记录器名称
            
        Returns:
            logging.Logger: 配置好的日志记录器实例
        """
        logger = logging.getLogger(name)
        
        # 如果已经配置过，直接返回
        if logger.handlers:
            return logger
        
        logger.setLevel(self.log_level)
        logger.propagate = False
        
        # 创建格式化器
        formatter = logging.Formatter(self.config['log_format'])
        
        # 添加控制台处理器
        if self.config['log_to_console']:
            console_handler = logging.StreamHandler(sys.stdout)
            console_handler.setFormatter(formatter)
            logger.addHandler(console_handler)
        
        # 添加文件处理器
        file_handler = RotatingFileHandler(
            self.log_file,
            maxBytes=self.config['max_log_size'],
            backupCount=self.config['backup_count'],
            encoding=self.config['encoding']
        )
        file_handler.setFormatter(formatter)
        logger.addHandler(file_handler)
        
        return logger


# 创建默认的日志记录器
logger_config = LoggerConfig()
logger = logger_config.get_logger('FakeNewsDetector')


def get_logger(name: str) -> logging.Logger:
    """
    获取指定名称的日志记录器
    
    Args:
        name: 日志记录器名称
        
    Returns:
        logging.Logger: 配置好的日志记录器实例
    """
    return logger_config.get_logger(name) 