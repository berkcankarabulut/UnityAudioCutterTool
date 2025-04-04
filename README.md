<h1>🎵 Unity Waveform Visualizer</h1>
<p>A simple <strong>waveform visualizer and audio trimming tool</strong> for Unity Editor, designed to help developers preview and manipulate audio clips.</p>
<h2>✨ Features</h2>
<ul>
<li>✔ <strong>Waveform Generation</strong> – Generates a visual representation of an audio clip’s amplitude.</li>
<li>✔ <strong>Selection Tool</strong> – Allows users to select a portion of the waveform for trimming.</li>
<li>✔ <strong>Interactive UI</strong> – Click and drag to adjust the <strong>start</strong> and <strong>end</strong> points dynamically.</li>
<li>✔ <strong>Customizable Appearance</strong> – Adjustable colors and styles for better readability.</li>
<li>✔ <strong>Supports Stereo & Mono Clips</strong> – Works with multi-channel audio files.</li>
</ul>
<h2>🎨 Visualization</h2>
<p>The waveform is rendered in the Unity <strong>Editor GUI</strong>, displaying the <strong>peak amplitude</strong> over time. Selection highlights the chosen portion of the clip. Vertical markers help <strong>track time positions</strong> within the waveform.</p>
<h2>📦 Installation</h2>
    <p>1. Clone this repository:</p>
    <pre><code>git clone https://github.com/berkcankarabulut/UnityAudioCutterTool.git</code></pre>
    <p>2. Add it to your Unity project.</p>
    <p>3. Configure the necessary settings.</p>
<h2>🖱️ How It Works</h2>
<ol>
<li>Load an <code>AudioClip</code>.</li>
<li>The script processes and <strong>samples the amplitude</strong> of the waveform.</li>
<li>Adjust <strong>start</strong> and <strong>end</strong> points using the mouse (<strong>Shift + Click</strong> for end point).</li>
<li>Use the processed data for <strong>trimming</strong>, <strong>saving</strong>, or <strong>previewing</strong> the selected clip.</li>
</ol>
<h2>📌 Dependencies</h2>
<ul>
<li>✅ Unity Editor (tested on <strong>2021+ versions</strong>)</li>
<li>✅ Requires <strong>Editor Window integration</strong> for GUI display</li>
</ul>

