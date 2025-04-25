% OFDMA 正交性推证仿真
clc; clear; close all;

% 参数设置
N = 64;                 % 子载波数（FFT点数）
Ts = 1e-3;              % 符号周期
fs = N / Ts;            % 采样率
t = linspace(0, Ts, N); % 时间向量
k1 = 5;                 % 第一个子载波索引
k2 = 15;                % 第二个子载波索引

% 生成两个正交子载波信号
s1 = exp(1j*2*pi*k1*t/Ts);  % 频率对应第k1个子载波
s2 = exp(1j*2*pi*k2*t/Ts);  % 频率对应第k2个子载波

% 合成总信号
s_total = s1 + s2;

% 接收端分别进行匹配滤波（点乘对应的共轭基函数并积分）
r1 = sum(s_total .* conj(s1));
r2 = sum(s_total .* conj(s2));
r_cross = sum(s1 .* conj(s2)); % 直接验证子载波之间的互相关

% 结果输出
disp(['r1（投影到子载波1） = ', num2str(r1)]);
disp(['r2（投影到子载波2） = ', num2str(r2)]);
disp(['r_cross（子载波1和子载波2互相关） = ', num2str(r_cross)]);

% 作图展示
figure;
subplot(3,1,1);
plot(t, real(s1)); title('子载波s1（实部）');
xlabel('时间 (s)'); ylabel('幅度'); grid on;

subplot(3,1,2);
plot(t, real(s2)); title('子载波s2（实部）');
xlabel('时间 (s)'); ylabel('幅度'); grid on;

subplot(3,1,3);
plot(t, real(s_total)); title('叠加信号s\_total（实部）');
xlabel('时间 (s)'); ylabel('幅度'); grid on;
