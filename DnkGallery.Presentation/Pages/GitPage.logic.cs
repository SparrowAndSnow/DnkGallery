﻿using DnkGallery.Model;
using DnkGallery.Model.Github;
using LibGit2Sharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Navigation;
using Path = System.IO.Path;
namespace DnkGallery.Presentation.Pages;

[UIBindable]
public sealed partial class GitPage : BasePage<BindableGitViewModel>, IBuildUI {
    private UIControls.GridView gridView;
    public GitPage() => BuildUI();
    
    
    
    private void ContentInvoke(UIControls.Page obj) {
    }
    
    protected override async void OnNavigatedTo(NavigationEventArgs e) {
        base.OnNavigatedTo(e);
        await vm?.Model?.Status();
    }
}

public partial record GitViewModel : BaseViewModel {
    public IListState<Ana> AddedAnas => ListState<Ana>.Empty(this);
    public IState<string> Message => State<string>.Value(this,() => "feat: add anas");
    public async Task Commit() {
        var message = await Message;
        if (string.IsNullOrWhiteSpace(message)) {
            InfoBarManager.Show(UIControls.InfoBarSeverity.Warning, GitPage.Header, "缺失提交信息");
            return;
        }
        var gitApi = Service.GetService<IGitApi>()!;
        await gitApi.Commit(Settings.LocalPath, message, new Identity(Settings.GitUserName, Settings.GitUserName));
    }
    
    public async Task Status() {
        try {
            var gitApi = Service.GetService<IGitApi>()!;
            var repositoryStatus = await gitApi.Status(Settings.LocalPath);
            if (repositoryStatus is null) {
                return;
            }
            var list = repositoryStatus.Added.Select(x => {
                var combine = Path.Combine(Settings.LocalPath, x.FilePath);
                var ana = new Ana(combine);
                return ana;
            });
            await AddedAnas.Update(_ => [..list], CancellationToken.None);
            
        } catch (Exception e) {
            InfoBarManager.Show(UIControls.InfoBarSeverity.Error, GitPage.Header, e.Message);
        }
        
    }
}
